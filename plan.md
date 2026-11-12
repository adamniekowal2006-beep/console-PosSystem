# Plan




## Plan ogólny
#### [] DataBase
Klasa apstrakcyjna jako szkielet do kazdego typu bazy danych, 
##### Posiadane pola
- [] Arrey jednowymiarowy typu T
- [] HashSet jednowymiarowy typu T

##### Metody


- [X] Konstruktor (ma przyjmowac liste i z tego robic list i hashset ())
- [X] GetList(out List<T> list)
- [X] SetList(List<T> list) 
- [X] Add(T item)
- [X] Delete(T item)
- [X] IsAny(T item)
- [X] Where(Func<T,bool>) //tu jescze moga byc zmiany
### DataBaseJson
Klasa apstrakcyjna posiadajace metody zapisu i wczytywania oraz validacji wartosci przed zapisem. Jednoczenisenie zapisuje backup i 

#### Posiadane Pola
- [] Path do danego elementu
- [] Ilosc wymaganych przechowan backupów

#### Metody
Uwaga! Metody nie maja walidowac niczego poza tym co robia (metody sprawdzaja tylko czy czegos same nie spaprały), maja robic tylko co jest im powiedziane maja robic
- [] Zapis bazy (Ma zapisać plik i stworzyc plik backup)
- [] Oddtworzenie bazy (Przy braku pliku lub błedzie ma poinformowac co sie stało)
- [] Oddtworzenie bazy z backupu (Przy braku pliku lub błedzie poinformuj urzytkownika)
- [] Wylistowanie backapów (nie jest to metoda asynchroniczna)

### Types


### StaticMetods

#### JsonMethods 
- [X] Serializacja plików
- [X] Deserializacja plików