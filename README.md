# AntizapretSOCKS5

## Описание

Инструмент для маршрутизации трафика приложений через SOCKS5-прокси Antizapret с использованием ProxiFyre.

## Структура

```
AntizapretSOCKS5/
├── ConfigEditor/       — GUI-приложение для редактирования конфигурации
├── Driver/             — Установщик Windows Packet Filter
├── ProxyAPP/           — ProxiFyre и конфигурация прокси
│   ├── ProxiFyre.exe
│   └── app-config.json
└── README.md
```

## ConfigEditor — Редактор конфигурации

Windows-приложение для удобного редактирования `app-config.json`.

### Возможности

- **Управление прокси-конфигурациями** — добавление, редактирование, удаление записей
- **Выбор приложений** — ввод названия вручную или выбор из списка запущенных процессов
- **Выбор прокси-сервера** — переключение между `socks-local.antizapret:8118` и `socks-world.antizapret:8118`
- **Настройка учётных данных** — имя пользователя и пароль для прокси
- **Выбор протоколов** — включение/выключение TCP и UDP
- **Запуск ProxiFyre** — кнопка для запуска ProxiFyre.exe после настройки
- **Проверка драйвера** — автоматическая проверка установки Windows Packet Filter при запуске с предложением установить

### Сборка

Требуется [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd ConfigEditor
dotnet publish -c Release
```

Результат — один файл `ConfigEditor.exe` со встроенными библиотеками в `bin/Release/net8.0-windows/win-x64/publish/`. Установка .NET на целевом компьютере не требуется.

### Использование

1. Поместите скомпилированный `ConfigEditor.exe` рядом с папками `ProxyAPP` и `Driver`
2. Запустите `ConfigEditor.exe`
3. При первом запуске программа проверит наличие драйвера Windows Packet Filter
4. Добавьте прокси-конфигурации через кнопку «Добавить»
5. Сохраните конфигурацию кнопкой «Сохранить конфиг»
6. Запустите ProxiFyre кнопкой «Запустить ProxiFyre»

### Пример конфигурации

```json
{
  "logLevel": "None",
  "proxies": [
    {
      "appNames": ["chrome", "chrome_canary"],
      "socks5ProxyEndpoint": "socks-world.antizapret:8118",
      "username": "username1",
      "password": "password1",
      "supportedProtocols": ["TCP", "UDP"]
    },
    {
      "appNames": ["firefox", "firefox_dev"],
      "socks5ProxyEndpoint": "socks-local.antizapret:8118",
      "username": "username2",
      "password": "password2",
      "supportedProtocols": ["TCP"]
    }
  ]
}
```