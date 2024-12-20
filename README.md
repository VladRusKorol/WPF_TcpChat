# Desktop TcpChat :speech_balloon:
## Клиентское :computer: и серверное :desktop_computer:	графические приложения для Windows 
![image](https://github.com/user-attachments/assets/e3909ac9-a483-4199-af0b-ca86acb92ad7)

> [!IMPORTANT]
> **Задача**:
> Реализовать графический интерфейс серверного приложения, который будет являться точкой приёмки и рассылки сообщений от клиентов. Также 
> реализовать графический интерфейс клиентского приложения, в котором можно подключаться к серверу, писать сообщения и получать сообщения от
> иных пользователей.

> [!WARNING] 
> **Необходимо применить**:
> - [x] Язык программирования C# и платформу разработки .NET
> - [x] Базовые инструменты для работы с сетями и протоколами
> - [x] Базовые инструменты для работы в асинхронном режиме

>[!NOTE]
> Технологию построения клиентских приложений (на выбор)

>[!TIP]
> **Примененные инструменты сервера:**
> * Язык программирования C# и платформу разработки .NET 8.0;
> * Для организации межсетевого взаимодействия использовались:
>   +  Протокол транспортного уровня **TCP**
>   +  Класс обертка над классом Socket: **TcpListener** (регистрация входящих подключений);
>   +  Класс обертка над классом Socket: **TcpClient** (прослушивание входящих сообщений от всех клиентов, отправка сообщений всем подключенным клиентам, подача команды на отключение хоста от сервера);
> * Для организации асинхронного режима использовались: 
>   +  Класс **Task**, (запуск в одном из потоков из пула потоков задачи по прослушиванию входящих сообщений, входящих подключений);
> * Технология построения клиентских приложений:
>   +  **WPF**(Windows Presentation Foundation) - является часть экосистемы платформы .NET и представляет собой подсистему для построения графических интерфейсов;
>   +  **MVVM** - паттерн разработки, позволяющий разделить приложение на три функциональные части:
>       + **Model** — основная логика программы (работа с данными, вычисления, запросы и так далее).
>       + **View** — вид или представление (пользовательский интерфейс).
>       + **ViewModel** — модель представления, которая служит прослойкой между View и Model.

>[!TIP]
> **Примененные инструменты клиента:**
> * Язык программирования C# и платформу разработки .NET 8.0;
> * Для организации межсетевого взаимодействия использовались:
>   +  Протокол транспортного уровня **TCP**
>   +  Класс обертка над классом Socket: **TcpClient** (прослушивание входящих сообщений от удаленного хоста, отправка сообщений серверу);
> * Для организации асинхронного режима использовались: 
>   +  Класс **Task**, (запуск в одном из потоков из пула потоков задачи по прослушиванию входящих сообщений);
> * Технология построения клиентских приложений:
>   +  **WPF**(Windows Presentation Foundation) - является часть экосистемы платформы .NET и представляет собой подсистему для построения графических интерфейсов;
>   +  **MVVM** - паттерн разработки, позволяющий разделить приложение на три функциональные части:
>       + **Model** — основная логика программы (работа с данными, вычисления, запросы и так далее).
>       + **View** — вид или представление (пользовательский интерфейс).
>       + **ViewModel** — модель представления, которая служит прослойкой между View и Model.


## Схема взаимодействия представлений
```mermaid
sequenceDiagram

    participant Server
    actor  Client_A
    actor  Client_B
    actor  Client_C

    rect rgb(191, 223, 255)
    note right of Client_A: Пользователь А отпраляет<br/> сообщение на сервер
    Client_A ->> Server: Сообщение от пользователя A
    Server -->> Client_A: Дубль сообщения пользователю A
    Server -->> Client_B: Сообщение от пользователя A
    end

    rect rgb(191, 255, 214)
    note left of Client_C: Пользователь подключился к серверу
    Client_C ->> Server: Пользователь С отправляет на сервер своё имя
    Server -->> Client_A: C вошел в чат
    Server -->> Client_B: C вошел в чат
    Server -->> Client_C: подтверждение что C <br/> вошел в чат
    end

    rect rgb(255, 181, 179)
    note left of Client_C: Пользователь отключился от сервера
    Client_C --x Server: Фиксация потери соединения
    note left of Server: Сервер получил уведомление<br/> об отключении пользователя С
    Server -->> Client_A: C покинул в чат
    Server -->> Client_B: C покинул в чат
    end  
```
## UML диаграмма серверного приложения
```mermaid
classDiagram
   direction RL
    class ChatServer{
        -TcpListener _listener
        -List~ChatClient~ _clients

        -int _countClientOnline
        -int _countClientConnected
        -int _countClientDisconnected
        -int _countMessageSend

        Task ServerStartAndListenAsync()
        void ServerStop()

        Task AcceptChatClientAsync()
        Task SendMessageAsync(string message)
        void OnCommandDeleteClient(Guid id)
        
    }

    class ChatClient{
            -TcpClient _client
            -string? _name;
            +Guid Id
            +StreamWriter? _writer 
            +StreamReader? _reader
            Task ProcessAsync()
            void Close()
    }
    style ChatServer fill:#fafafa,stroke:#333,stroke-width:2px
    note for ChatServer "Главный класс серверного приложения"

    

    ChatServer --> ChatClient : Создает объекты ChatClient, и добавляет их в лист
    ChatServer <|--|> ChatClient : Подписка OnMessageNotify
    ChatServer <|--|> ChatClient : Подписка OnDeleteClientNotify


    class MainWindowViewModel{
        +string? Port
        +string? IpAddress
        +ServerStatus Status
        +string StatusMSG
        +SolidColorBrush StatusForegroudColor
        +int CurrentClientOnline
        +int CountConnectedUsers
        +int CountDisconnectedUsers
        +int CountsSendMessages
        void CountConnectedClient(int count)
        void CountDisconnectedClient(int count)
        void CountSendMessage(int count)
    }

     MainWindowViewModel --> ChatServer : Создает объекты ChatServer инициализируя его из даными из графический оболочки
     MainWindowViewModel <|--|> ChatServer : Подписка CountClientConnect
     MainWindowViewModel <|--|> ChatServer : Подписка CountClientDisconnect
     MainWindowViewModel <|--|> ChatServer : Подписка CountSendMessage
     MainWindowViewModel <|--|> ChatServer : Подписка CountClientOnline
```
## UML диаграмма клиентского приложения
```mermaid
    classDiagram
    direction RL
    class ClientClass{
        -string _ip
        -int _port
        -string _username
        -TcpClient _client
        -bool _isWorking
        -StreamReader _reader
        -StreamWriter _writer
        +event OnReceiveMessageEvent
        +event OnClientIsConnected
        +void ConnectAsync()
        +Task ReceiveMessageAsync()
        +Task SendMessageAsync(string message)
        +void Disconnect()
    }
    style ClientClass fill:#fafafa,stroke:#333,stroke-width:2px
    note for ClientClass "Главный класс клиентского приложения"
    class MainWindowViewModel{
        +string? Port
        +string? Ip
        +ClientStatus Status
        +string? UserName
        +SolidColorBrush StatusForegroudColor
        +string? Message
        +ObservableCollection<string> _chatElements
        +void OnClientIsConnected(bool isConnected)
        +void OnRecivedMessage(string message)
    }
    MainWindowViewModel --> ClientClass : Создает объекты ClientClass инициализируя его из даными из графический оболочки
     MainWindowViewModel <|--|> ClientClass : Подписка OnReceiveMessageEvent
     MainWindowViewModel <|--|> ClientClass : Подписка OnClientIsConnected
    
```



