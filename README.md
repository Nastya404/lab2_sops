# Лаб. 2 — WCF (SOAP) и gRPC

Клиент-серверное приложение для таблицы `roster` (хоккейные игроки).

- **Сервер** хранит все данные (SQLite `CRITTERS.db`), проверяет записи и сохраняет их.
  Доступ к нему — двумя способами:
  - **2.1 — WCF**, привязка `BasicHttpBinding` (SOAP 1.1 по HTTP);
  - **2.2 — gRPC**, HTTP/2 без TLS.
- **Клиент** — веб-приложение (ASP.NET MVC). **Ничего не хранит и не проверяет**: список игроков
  получает со службы, новую запись отправляет на службу и показывает ответ (сохранено / ошибки).

```
 Компьютер 2 — клиент                         Компьютер 1 — сервер
 CrittersApp  профиль wcf  :5080 ── SOAP ──>  CrittersWcfService   :5100 ─┐
 CrittersApp  профиль grpc :5081 ── gRPC ──>  CrittersGrpcService  :5200 ─┤
 (только веб-интерфейс, без БД)                                           └─> Critters.Shared
                                                                              правила + EF Core
                                                                              C:\sops\CRITTERS.db
```

Обе службы работают с **одной и той же** БД: игрок, добавленный через WCF, виден через gRPC и наоборот.

## Важно знать

- Нужен **.NET 9 SDK** на обоих компьютерах (проверка: `dotnet --version` → `9.x`).
- На **оба** компьютера нужна **вся папка решения целиком**: клиент при сборке берёт контракт
  `CrittersGrpcService/Protos/roster.proto` из папки службы.
- Перед переносом через GitHub сначала **закоммить и запушить все изменения**
  (`git add -A`, `git commit`, `git push`), потом на втором компьютере `git pull`.
- Все команды `dotnet run` выполняются **из папки проекта** (`cd ...` перед запуском), иначе будет
  ошибка `MSB1009: файл проекта не существует`.
- Компьютеры должны быть **в одной сети** и видеть друг друга по IP.
- БД — **только на сервере**: `CRITTERS.db` в корне решения. Смотреть данные — там
  (DB Browser for SQLite), а не на клиенте. Пока БД открыта в DB Browser с несохранёнными изменениями,
  службы не смогут в неё записать — закрывайте или нажимайте «Записать изменения».
- Порты клиента — **5080/5081**, а не 5000: на macOS порт 5000 занят системным приёмником AirPlay.

| Проект | Где запускается | Назначение | Порт |
|---|---|---|---|
| `Critters.Shared` | сервер (библиотека) | сущность `Player`, правила `PlayerRules`, хранилище `PlayerStore` (EF Core + SQLite) | — |
| `CrittersWcfService` | сервер | WCF-служба `RosterService.svc` | 5100 |
| `CrittersGrpcService` | сервер | gRPC-служба `Roster` | 5200 |
| `CrittersApp` | клиент | веб-интерфейс; обращается к серверу по WCF **или** gRPC | 5080 (wcf) / 5081 (grpc) |

Решение целиком: `Critters.sln` (`dotnet build Critters.sln`).

## Операции служб

Одинаковые для WCF и gRPC:

| Операция | Что делает |
|---|---|
| `GetPlayers` | возвращает всех игроков из БД сервера |
| `AddPlayer(player)` | проверяет запись (поля, 4 правила, уникальность ID и номера) и сохраняет. Возвращает список ошибок `{Field, Message}`; пустой — запись сохранена |
| `GetOptions` | допустимые позиции и штаты (для выпадающих списков формы) |

## Запуск на двух компьютерах

### Компьютер 1 — сервер (службы + БД)

1. Узнать IP-адрес:
   ```powershell
   ipconfig
   ```
   Строка «IPv4-адрес» адаптера Wi-Fi/Ethernet, например `192.168.1.42`.
   Ниже вместо `192.168.1.42` подставляй свой адрес.

2. Открыть порты в брандмауэре — **один раз**, PowerShell **от имени администратора**:
   ```powershell
   New-NetFirewallRule -DisplayName "Critters WCF"  -Direction Inbound -Protocol TCP -LocalPort 5100 -Action Allow
   New-NetFirewallRule -DisplayName "Critters gRPC" -Direction Inbound -Protocol TCP -LocalPort 5200 -Action Allow
   ```

3. Запустить службы — каждую в **отдельном** терминале, терминалы не закрывать:
   ```powershell
   cd C:\sops\CrittersWcfService
   dotnet run
   ```
   ```powershell
   cd C:\sops\CrittersGrpcService
   dotnet run
   ```
   В консоли появится путь к БД и адреса для подключения:
   ```
   info: БД: C:\sops\CRITTERS.db
   info: WCF-служба доступна по адресу: http://192.168.1.42:5100/RosterService.svc
   ```

4. Проверить на самом сервере в браузере: `http://localhost:5100/RosterService.svc?wsdl` — открывается WSDL.

### Компьютер 2 — клиент (веб-интерфейс)

1. Проверить, что сервер доступен.

   Windows (PowerShell):
   ```powershell
   Test-NetConnection 192.168.1.42 -Port 5100
   Test-NetConnection 192.168.1.42 -Port 5200
   ```
   Нужно `TcpTestSucceeded : True`.

   Mac (Терминал):
   ```bash
   nc -vz 192.168.1.42 5100
   nc -vz 192.168.1.42 5200
   ```
   Нужно `succeeded`.

   В браузере `http://192.168.1.42:5100/RosterService.svc?wsdl` должен показать WSDL.

2. В `CrittersApp/appsettings.json` заменить `localhost` на IP сервера (две строки):
   ```json
   "RosterService": {
     "Transport": "Wcf",
     "WcfUrl": "http://192.168.1.42:5100/RosterService.svc",
     "GrpcUrl": "http://192.168.1.42:5200",
     "TimeoutSeconds": 10
   }
   ```
   `Transport` трогать не нужно — вариант выбирается профилем запуска (шаг 3).

3. Запустить нужный вариант (можно оба сразу, каждый в своём терминале):
   ```bash
   cd C:\sops\CrittersApp          # на Mac: cd ~/<папка репозитория>/CrittersApp
   dotnet run --launch-profile wcf
   ```
   → вариант 2.1, открыть в браузере `localhost:5080`
   ```bash
   cd C:\sops\CrittersApp
   dotnet run --launch-profile grpc
   ```
   → вариант 2.2, открыть в браузере `localhost:5081`

   Ссылку `http://[::]:5080` / `http://0.0.0.0:5080` из терминала открывать **не нужно** — это не адрес
   сайта, а «слушать на всех сетевых картах». Набирать `localhost:5080`.

   В Visual Studio профиль (`wcf` / `grpc`) выбирается в списке рядом с кнопкой запуска.

   Адрес сервера можно передать и без правки `appsettings.json`:
   ```bash
   dotnet run --launch-profile grpc -- --RosterService:GrpcUrl=http://192.168.1.42:5200
   ```

4. Проверить: в шапке сайта — «Сервер: **Wcf** (http://192.168.1.42:5100/RosterService.svc)».
   Если там `localhost` — IP в `appsettings.json` не сохранён.
   Добавить игрока → он появляется в списке, в консоли службы на сервере строка `AddPlayer ... -> СОХРАНЁН`,
   а в `C:\sops\CRITTERS.db` на сервере — новая запись.

Чтобы открыть сайт с третьего компьютера, на клиенте открыть порты 5080/5081 и заходить по `http://IP-клиента:5080`.

### Всё на одном компьютере

Ничего менять не нужно (в `appsettings.json` уже `localhost`): запустить обе службы, затем
`dotnet run --launch-profile wcf` / `grpc` в папке `CrittersApp`.

### Журнал службы: видно, что запросы идут с другого компьютера

Обе службы пишут в консоль каждый запрос — **IP клиента**, данные и результат:

```
20:14:34 info: CrittersWcfService.RosterService[0] SOAP GetPlayers от 192.168.1.50: 29 игроков
20:14:40 info: CrittersWcfService.RosterService[0] SOAP AddPlayer от 192.168.1.50: t1 #90 A B, LW, 1995-01-01, 180 см, 80 кг, X NY -> СОХРАНЁН
20:14:41 info: CrittersGrpc.Services.RosterGrpcService[0] gRPC AddPlayer от 192.168.1.50: t2 #90 A B, G, 1995-01-01, 170 см, 70 кг, X NY -> отклонён [Вратарь (G) должен иметь рост не менее 180 см. | ...]
```

`192.168.1.50` — IP компьютера-клиента; `127.0.0.1` — запрос с этого же компьютера.
Среди адресов при запуске могут быть `192.168.x.1` от VMware/VirtualBox — для подключения нужен адрес Wi-Fi/Ethernet.

### Если не работает

| Что видно | Причина / что делать |
|---|---|
| «Сервер недоступен: …отверг запрос…» | служба не запущена или на сервере закрыт порт в брандмауэре |
| «Сервер недоступен» появляется только через ~10 секунд | неверный IP или компьютеры в разных сетях |
| `TcpTestSucceeded : False` / `nc` не `succeeded` | брандмауэр сервера / неверный IP / служба не запущена |
| `MSB1009: файл проекта не существует` | команда запущена не из папки проекта — сначала `cd` |
| сборка падает с «file is locked by …» | программа уже запущена — закрыть её старый терминал (Ctrl+C) |
| в `appsettings.json` нет секции `RosterService` | на компьютере старая версия кода — `git pull` / скопировать папку заново |
| `--launch-profile wcf` не найден / Mac: `address already in use` на 5000 | старая версия `launchSettings.json` — обновить код |
| запись «сохранилась», но в БД её нет | смотрите `CRITTERS.db` **на сервере** (путь — в консоли службы, строка `БД:`) |
| ошибка `database is locked` в консоли службы | БД открыта в DB Browser с несохранёнными изменениями |

## Как устроено

### Сервер

```
Critters.Shared/
  Player.cs              сущность таблицы roster: маппинг столбцов + правила полей (Required, Range...)
  PlayerRules.cs         полная проверка: поля, списки, диапазон даты, 4 правила предметной области
  PlayerLookups.cs       допустимые позиции и штаты
  Data/CrittersContext   EF Core DbContext
  Data/PlayerStore       GetAll / Add (проверка + уникальность ID и номера + сохранение)
CrittersWcfService/
  IRosterService.cs      контракт: [ServiceContract], DataContract PlayerDto/FieldErrorDto/OptionsDto
  RosterService.cs       реализация + маппинг PlayerDto <-> Player + журнал
CrittersGrpcService/
  Protos/roster.proto    контракт gRPC (один файл и для сервера, и для клиента)
  Services/RosterGrpcService.cs  реализация + маппинг protobuf <-> Player + журнал
```

Путь к БД — `"Database"` в `appsettings.json` служб (по умолчанию `../CRITTERS.db`, т.е. корень решения).

Правила проверки (`Critters.Shared/PlayerRules.cs`):

1. Вратарь (G): рост ≥ 180 см, вес ≥ 80 кг.
2. Центральный нападающий (C), родившийся до 2000 года: номер < 15.
3. Игрок из штата MA ростом > 185 см: вес ≥ 90 кг.
4. Защитник (D) младше 25 лет ростом < 185 см: вес ≤ 85 кг.
5. Дата рождения — с 01.01.1980 по 31.12.2016; все поля обязательны; ID и номер уникальны.

### Клиент

```
CrittersApp/
  Controllers/RosterController.cs  список / добавление, ошибки сервера выводит в форму
  Models/Player.cs                 модель формы (без БД и без правил)
  Services/IRosterClient.cs        общий интерфейс клиента службы
  Services/WcfRosterClient.cs      вариант 2.1, клиент WCF-службы через прокси-класс RosterServiceReference/Reference.cs
  Services/GrpcRosterClient.cs     вариант 2.2, клиент gRPC-службы (код генерируется из roster.proto)
  Services/RosterMapping.cs        маппинг Player <-> PlayerDto (WCF) и Player <-> protobuf Player (gRPC)
  Services/RosterServiceOptions    настройки: Transport (Wcf | Grpc), адреса служб, таймаут
```

## WSDL

Файл: [RosterService.wsdl](RosterService.wsdl) (получен через `…/RosterService.svc?singleWsdl`;
`?wsdl` отдаёт то же самое, но со схемами XSD в отдельных документах `?xsd=xsd0..2`).

- `wsdl:types` — XSD-схемы:
  - тип `PlayerDto` (10 полей; `int?` → `xs:int nillable="true"`, `DateTime?` → `xs:dateTime nillable="true"`),
    `ArrayOfPlayerDto` (это `PlayerDto[]`);
  - `FieldErrorDto` (`Field`, `Message`) и `ArrayOfFieldErrorDto`;
  - `OptionsDto` (`Positions`, `States` — `ArrayOfstring`);
  - элементы-обёртки запросов/ответов: `GetPlayers`/`GetPlayersResponse`, `AddPlayer`(параметр `player`)/`AddPlayerResponse`,
    `GetOptions`/`GetOptionsResponse`.
- `wsdl:message` — входное/выходное сообщение каждой операции (`IRosterService_AddPlayer_InputMessage` и т.д.).
- `wsdl:portType IRosterService` — абстрактный интерфейс (= `[ServiceContract]`), операции `GetPlayers`, `AddPlayer`, `GetOptions`.
- `wsdl:binding BasicHttpBinding_IRosterService` — привязка к SOAP 1.1 по HTTP
  (`soap:binding transport=".../soap/http"`), стиль `document/literal`, у каждой операции свой
  `soapAction`, например `http://critters.example/roster/IRosterService/AddPlayer`.
- `wsdl:service / wsdl:port` — адрес конечной точки `soap:address`. Благодаря
  `UseRequestHeadersForMetadataAddressBehavior` в адрес подставляется хост из запроса (IP сервера, а не localhost).

## Прокси-класс

Файл: [CrittersApp/RosterServiceReference/Reference.cs](CrittersApp/RosterServiceReference/Reference.cs),
сгенерирован `dotnet-svcutil` по WSDL (параметры — `dotnet-svcutil.params.json`). Перегенерация
(WCF-служба должна быть запущена; после генерации удалить из `CrittersApp.csproj` добавленный утилитой
пакет `System.ServiceModel.NetTcp` — он не нужен):

```
cd CrittersApp
dotnet-svcutil http://localhost:5100/RosterService.svc?wsdl -n "*,CrittersApp.RosterServiceReference" -d RosterServiceReference -o Reference.cs
```

Содержимое:

- `PlayerDto`, `FieldErrorDto`, `OptionsDto` — клиентские копии `DataContract` из `wsdl:types`
  (тот же `Namespace`, поэтому XML совпадает с серверным).
- `IRosterService` — интерфейс контракта из `wsdl:portType`; `Action/ReplyAction` взяты из WSDL;
  операции генерируются асинхронными (`GetPlayersAsync`, `AddPlayerAsync`, `GetOptionsAsync`).
- `IRosterServiceChannel` — контракт + `IClientChannel` (управление каналом).
- `RosterServiceClient : ClientBase<IRosterService>` — сам прокси: создаёт канал,
  сериализует вызов в SOAP-конверт и отправляет его. `GetBindingForEndpoint` создаёт `BasicHttpBinding`
  (как в `wsdl:binding`), `GetEndpointAddress` — адрес из `soap:address`. В приложении используется
  конструктор `(EndpointConfiguration, string remoteAddress)`, чтобы адрес брался из `appsettings.json`.
