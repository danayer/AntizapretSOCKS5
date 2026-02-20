# AntizapretSOCKS5

## Описание

Инструмент для маршрутизации трафика приложений через SOCKS5-прокси Antizapret с использованием [ProxiFyre](https://github.com/wiresock/proxifyre) — мощного фильтра пакетов Windows для маршрутизации трафика через прокси.

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

## Использование с Antizapret VPN Docker

Этот инструмент можно использовать совместно с [antizapret-vpn-docker](https://github.com/xtrime-ru/antizapret-vpn-docker) для локального доступа к SOCKS5-прокси.

### Сценарии использования

**Вариант 1: Локальный SOCKS5 прокси (socks-local.antizapret)**
- Используйте `socks-local.antizapret:8118` для доступа к местным ресурсам
- Подходит для быстрого доступа к заблокированным локальным сайтам
- Требует запущенного контейнера antizapret-vpn-docker

**Вариант 2: Мировой SOCKS5 прокси (socks-world.antizapret)**
- Используйте `socks-world.antizapret:8118` для полного маршрутирования трафика через прокси
- Подходит для приватности и обхода геоблокировок
- Требует корректной конфигурации Docker-контейнера

### Подготовка окружения

1. Разверните [antizapret-vpn-docker](https://github.com/xtrime-ru/antizapret-vpn-docker) на VPS согласно его документации
2. Подключитесь к VPN на VPS с помощью WireGuard или OpenVPN (согласно конфигурации antizapret-vpn-docker)
3. Убедитесь, что SOCKS5-прокси доступны на портах `8118` после подключения к VPN
4. Проверьте возможность подключения: `ping socks-local.antizapret`
5. Настройте учётные данные в ConfigEditor согласно конфигурации контейнера на VPS

## Технические детали

- **ProxiFyre** — основной компонент для фильтрации и маршрутизации трафика на уровне ОС Windows
- **Windows Packet Filter** — драйвер, обеспечивающий перехват сетевых пакетов
- **ConfigEditor** — GUI для удобного управления конфигурацией приложений и их маршрутами через SOCKS5
