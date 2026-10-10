# Лаб. 2 — WCF (SOAP) и gRPC

| Проект | Назначение | Порт |
|---|---|---|
| `Critters.Shared` | доменные правила проверки записи (`RosterData`, `RosterRules`), общие для обеих служб | — |
| `CrittersWcfService` | WCF-служба (CoreWCF), `BasicHttpBinding` = SOAP 1.1 | 5100 |
| `CrittersGrpcService` | gRPC-служба (HTTP/2 без TLS), контракт `Protos/roster_validation.proto` | 5200 |
| `CrittersApp` | MVC-приложение (таблица roster, SQLite). Вызывает службу проверки по WCF **или** gRPC | 5000 / 5001 |

Решение целиком: `Critters.sln` (`dotnet build Critters.sln`).

Структура `CrittersApp`:

```
Controllers/RosterController.cs   список и добавление игроков
Data/CrittersContext.cs           EF Core DbContext
Models/Roster.cs                  сущность таблицы roster (маппинг столбцов)
Models/RosterLookups.cs           допустимые позиции и штаты
Services/IRosterValidationClient  общий интерфейс клиента службы
Services/RosterMapping.cs         маппинг Roster -> RosterDto (WCF) и Roster -> RosterRequest (gRPC)
Services/Wcf/                     вариант 2.1, использует прокси-класс ValidationServiceReference/Reference.cs
Services/Grpc/                    вариант 2.2, клиент генерируется из .proto службы
Services/ValidationOptions.cs     настройки: Transport (Wcf | Grpc), адреса служб, таймаут
```

Цепочка маппинга данных:

```
Roster (EF, таблица roster)
  ├─ WCF:  RosterMapping.ToWcfDto()     -> RosterDto (DataContract из прокси) -> SOAP XML
  │        ValidationService.ToRosterData()       -> RosterData -> RosterRules.Validate
  └─ gRPC: RosterMapping.ToGrpcRequest() -> RosterRequest (protobuf)
           RosterValidationService.ToRosterData() -> RosterData -> RosterRules.Validate
```

## Запуск на двух компьютерах

**Компьютер-сервер** (службы):

1. Узнать IP: `ipconfig` (например `192.168.1.42`).
2. Открыть порты в брандмауэре (PowerShell от администратора):
   ```powershell
   New-NetFirewallRule -DisplayName "Critters WCF"  -Direction Inbound -Protocol TCP -LocalPort 5100 -Action Allow
   New-NetFirewallRule -DisplayName "Critters gRPC" -Direction Inbound -Protocol TCP -LocalPort 5200 -Action Allow
   ```
3. Запустить службы (каждую в отдельном терминале; путь — папка, куда скопировано решение):
   ```
   cd C:\sops\CrittersWcfService
   dotnet run
   ```
   ```
   cd C:\sops\CrittersGrpcService
   dotnet run
   ```
4. Проверка с клиента в браузере: `http://192.168.1.42:5100/ValidationService.svc?wsdl`.

**Компьютер-клиент** (веб-приложение): в `CrittersApp/appsettings.json`, секция `Validation`, указать IP сервера:

```json
"Validation": {
  "Transport": "Wcf",
  "WcfUrl": "http://192.168.1.42:5100/ValidationService.svc",
  "GrpcUrl": "http://192.168.1.42:5200",
  "TimeoutSeconds": 10
}
```

Вариант выбирается профилем запуска (`Properties/launchSettings.json`):

```
cd C:\sops\CrittersApp
dotnet run --launch-profile wcf      # вариант 2.1, http://localhost:5000
dotnet run --launch-profile grpc     # вариант 2.2, http://localhost:5001
```

Оба профиля можно запустить одновременно, каждый в своём терминале. В Visual Studio профиль выбирается
рядом с кнопкой запуска. Какая служба используется сейчас, видно в шапке сайта.

Адрес можно передать и без правки файлов:

```
dotnet run --launch-profile grpc -- --Validation:GrpcUrl=http://192.168.1.42:5200
```

Веб-приложение слушает все интерфейсы, поэтому его тоже можно открыть с другого компьютера —
для этого откройте в брандмауэре порты 5000/5001.

## WSDL

Файл: [ValidationService.wsdl](ValidationService.wsdl) (получен через `…/ValidationService.svc?singleWsdl`;
`?wsdl` отдаёт то же самое, но со схемами XSD в отдельных документах `?xsd=xsd0..2`).

- `wsdl:types` — XSD-схемы: элемент `Validate` (параметр `roster` типа `RosterDto`),
  тип `RosterDto` (7 полей; `int?` → `xs:int nillable="true"`, `DateTime` → `xs:dateTime`),
  ответ `ValidateResponse` → `ValidateResult` типа `ArrayOfstring` (это `string[]`).
- `wsdl:message` — входное/выходное сообщение операции (`IValidationService_Validate_InputMessage/OutputMessage`).
- `wsdl:portType IValidationService` — абстрактный интерфейс (= `[ServiceContract]`), операция `Validate`.
- `wsdl:binding BasicHttpBinding_IValidationService` — привязка к SOAP 1.1 по HTTP
  (`soap:binding transport=".../soap/http"`), стиль `document/literal`,
  `soapAction="http://critters.example/validation/IValidationService/Validate"`.
- `wsdl:service / wsdl:port` — адрес конечной точки `soap:address`. Благодаря
  `UseRequestHeadersForMetadataAddressBehavior` в адрес подставляется хост из запроса (IP сервера, а не localhost).

## Прокси-класс

Файл: [CrittersApp/ValidationServiceReference/Reference.cs](CrittersApp/ValidationServiceReference/Reference.cs),
сгенерирован `dotnet-svcutil` по WSDL (параметры — `dotnet-svcutil.params.json`). Перегенерация (служба должна быть запущена):

```
cd CrittersApp
dotnet-svcutil http://localhost:5100/ValidationService.svc?wsdl -n "*,CrittersApp.ValidationServiceReference" -d ValidationServiceReference -o Reference.cs
```

Содержимое:

- `RosterDto` — клиентская копия `DataContract` из `wsdl:types` (тот же `Namespace`, поэтому XML совпадает с серверным).
- `IValidationService` — интерфейс контракта из `wsdl:portType`; `Action/ReplyAction` взяты из WSDL;
  операция генерируется асинхронной (`ValidateAsync`).
- `IValidationServiceChannel` — контракт + `IClientChannel` (управление каналом).
- `ValidationServiceClient : ClientBase<IValidationService>` — сам прокси: создаёт канал,
  сериализует вызов в SOAP-конверт и отправляет его. `GetBindingForEndpoint` создаёт `BasicHttpBinding`
  (как в `wsdl:binding`), `GetEndpointAddress` — адрес из `soap:address`. В приложении используется
  конструктор `(EndpointConfiguration, string remoteAddress)`, чтобы адрес брался из `appsettings.json`.
