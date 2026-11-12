# Plan




## Plan ogólny
#### [] DataBase
Klasa apstrakcyjna jako szkielet do kazdego typu bazy danych, 
##### Posiadane pola
- [] Arrey jednowymiarowy typu T
- [] HashSet jednowymiarowy typu T

##### Metody
Uwaga! Funckcje nie moga zostac wykonane jezeli nie zostalo poprawnie zainicjowane

- [X] Konstruktor (ma przyjmowac liste i z tego robic list i hashset ())
- [] TryGetAsync(out List<T> list)
- [] TrySetAsync(List<T> list) -- Przemyslec czy te dwie funckje maja byc async
- [] Add(T item)
- [] Delete(T item)
- [] IsAny(T item)
- [] Select(Func<T,bool>) // tu jeszcze moga byc zmiany
- [] Search(Func<T,bool>) //tu jescze moga byc zmiany
### DataBaseJson
Klasa apstrakcyjna posiadajace metody zapisu i wczytywania oraz validacji wartosci przed zapisem. Jednoczenisenie zapisuje backup i 

#### Posiadane Pola
- [] Path do danego elementu
- [] Ilosc wymaganych przechowan backupów

#### Metody
- [] Zapis bazy
- [] Oddtworzenie bazy
- [] Oddtworzenie bazy z backupu
- [] Wylistowanie backapów

### Types


### StaticMetods

#### JsonMethods 
- [X] Serializacja plików
- [X] Deserializacja plików