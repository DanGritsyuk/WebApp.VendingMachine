# WebApp.VendingMachine
Emulation work Vending Machine for drinks

Согласно выданному ТЗ были выполнены следующие пункты:

+ Веб-приложение имитирует работу автомата по продаже напитков. Напитки представлены названием, картинкой и стоимостью.
+ Автомат принимает монеты номиналом в 1, 2, 5, 10 рублей. Есть перспектива расширить номинал. Приложение предоставляет возможность купить напитки, внеся сумму, равную или превышающую их стоимость.
+ Приложение имеет пользовательский и административный интерфейс:

	Пользовательский интерфейс отображает список доступных напитков и дает возможность внести монеты отображает внесенную, зарезервированную и остаточную суммы;

	При первом добавлении монетки пользователем, приложение создает клиентские данные и кеширует и на сервере, а пользователь получает ids своей «корзины» и сессии, где она хранится;

	Корзина и сессии удаляются в случае, когда пользователь завершил или отменил покупку. Также удаление данных происходит, если клиент не был активен в течении длительного времени, в этом случае, монеты, внесенные пользователем, не возвращаются;
Далее, клиент может отменить покупку или завершить её;

	В случае, если пользователь отменил покупки ему возвращается его сумма, однако, не обязательно теме же монетами;
В случае, если пользователь подтвердил покупку, ему отображается «чек» и возвращенные монеты в качестве сдачи, если таковая имеется;
Пользователь не может выбрать товар если остаток баланса меньше стоимости или если товар заблокирован администратором; 
Пользователь не может добавить монету номинал которой заблокировал администратор;

	Пользователь может получить в качестве сдачи, заблокированные для внесения монеты.

+ Административный интерфейс предоставляет инструменты для управления автоматом:

	Для того, чтобы попасть в раздел администратора, необходимо пройти по ссылке:
	<домен>/Admin/Index/?user=Administrator&AccessKey=5945bada-e7a4-4031-9656-558b5fcfd79c

	При первом запуске, приложение предложит заполнить поле с названием и установить торговый автомат (ТА). Имя и уникальный номер автомата сохраняются в базу данных, кроме того приложение запоминает свой уникальный номер в файле «.config», расположенный в разделе App_Data/Config/ (там же содержится файл с ключом доступа к администрированию).  Благодаря чему приложение может найти свои записи в таблицах. Тем самым администратор получает возможность хранить данные множества ТА в одной базе данных. 
Создав новую ТА, в разделе администрирования появляются следующие возможности:
Добавление, редактирование, удаление и просмотр данных напитков, кроме этого реализована возможность экспорта и импорта таких данных. Для этого приложение сериализует выбранные объекты из базы данных в файл и сохраняет его в «App_Data/Temp», после чего собирает все изображения экспортируемых объектов в туже папку. Далее происходит архивация данных в файл «.vm». При импорте наоборот, получив файл, приложение распаковывает его в папку «App_Data/Temp», после чего предлагает просмотреть импортируемые объекты и выбрать нужные. В случае, если название импортируемого товара совпадает с уже существующим, название сменит цвет на красный, при импорте такого объекта он перезапишет существующий в базе.
В архив добавил такой файл с данными, для быстрого заполнения.

	Еще одна реализованная администраторская функция — управление монетами в ТА. Администратор может блокировать или разблокировать те или иные номиналы монет и менять их количество.

	И последнее, в ТА предусмотрена выдача сдачи самыми накопившимися монетами в «лотке». Вычисления производится по функции y = Sqrt(x), где «x» – разница денежной суммы каждого из доступных номиналов монет и самого малого наминала в ТА; по значению получившегося «y», выстраивается список приоритетных в качестве сдачи монет. В случае, если значения «y» нескольких номиналов равны, в приоритете больший.

	- Из не реализованного:

Не успел протестить все возможные исключения с выдачей сдачи. Сейчас лучше всего монете номиналом 1 не ставить маленькое количество. Если они закончаться, может выпасть исключение о невозможности выдачи сдачи;

Защищенная ссылка работает только в Admin/Index. В производных страницах администратора не успел наладить;

Не успел провести более тщательно ревью кода.