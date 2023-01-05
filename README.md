# SAFETY

**Веб-приложение родительского контроля на ASP.NET Core MVC.** 

***В приложении реализована система аутентификации на основе логина и пароля.*** Основной функционал программы доступен только аутентифицированным пользователям. ***Данные пользователей хранятся в базе данных, пароли хешируются по алгоритму MD5.*** Незарегистрированные пользователи могут пройти регистрацию и получить доступ к приложению. При входе в приложение информация о принципале сохраняется в файл куки.  

![1](https://github.com/Presstomsk/Web_Parent_Control/blob/master/jpg/Auth.jpg)

![2](https://github.com/Presstomsk/Web_Parent_Control/blob/master/jpg/Registration.jpg)

В приложении доступен функционал по просмотру веб-страниц, которые были посещены пользователем в течении месяца (пункт меню приложения "Журнал"), и файлов, скачанных пользователем за последний месяц (пункт меню приложения "Загрузки"). ***При желании, пользователь может провести блокировку сайтов с подозрительным или нежелательным контентом, нажав соответствующую кнопку "Блокировать сайт".*** Кнопка изменит цвет c красного на зеленый и текст на "Разблокировать сайт". ***При нажатии на кнопку блокируется не страница, а веб-сайт целиком.*** Поэтому, если в списке есть страницы, принадлежащие тому же сайту, их кнопки тоже изменятся. ***Блокировка/разблокировка осуществляется посредством технологии ajax.***

Используя фильтр, можно сделать выборку за месяц, за неделю или за день. 

***Приложение осуществляет работу путем взаимодействия с Web API приложения [ParentSpy](https://github.com/Presstomsk/Parent_Spy).*** Функционал приложения ParentSpy доступен только доверенным пользователям, аутентификация пользователя подтверждается JWT-токеном. ***Токен генерируется в приложении SAFETY и отправляется в заголовке запроса в ParentSpy.***

![3](https://github.com/Presstomsk/Web_Parent_Control/blob/master/jpg/Blocking.png)

***Список всех заблокированных сайтов сохраняется в БД и доступен в приложении*** (пункт меню приложения "Блокированные сайты").

![4](https://github.com/Presstomsk/Web_Parent_Control/blob/master/jpg/BlockedSites.jpg)

При желании пользователь может сменить пароль (пункт меню приложения "Изменить пароль").

![5](https://github.com/Presstomsk/Web_Parent_Control/blob/master/jpg/ChangePass.jpg)

***При выходе из приложения файл куки удаляется, список сайтов и файлов в целях безопасности удаляется из БД.*** В БД на постоянной основе хранятся только данные о пользователях и список блокированных сайтов. 
