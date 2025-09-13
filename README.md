EMAdvertisementApi

ASP.NET Core Web API (.NET 8) для хранения и поиска рекламных площадок по локации.
Данные загружаются из текстового файла (.txt) и хранятся в оперативной памяти.
Реализован поиск списка рекламных площадок для заданной локации.

Требования
.NET SDK 8.0
Текстовый файл с локациями (прикреплен к проекту)

Содержание файла в формате: 
НазваниеПлощадки1:лок1,лок2,лок3 ..., локn
НазваниеПлощадки2:лок1,лок2,лок3 ..., локn

Инструкция:
Загрузить файл в метод api/Advertismenet/upload
Выполнить поиск с указанием локации в методе api/Advertisement

Пример.
1)Загрузка данных из файла Locations.txt
<img width="1530" height="575" alt="image" src="https://github.com/user-attachments/assets/d1ca076c-470a-4b66-aca8-42c20ae5459a" />

2)Информация о выполнении 
<img width="1511" height="698" alt="image" src="https://github.com/user-attachments/assets/d63b702a-e141-43a0-9498-9a507b5e3006" />

3)Ввести локацию для поиска в get метод
<img width="1469" height="875" alt="image" src="https://github.com/user-attachments/assets/d03930d2-f4db-4e2f-b73e-e541d7bf7d72" />

