# Vrh.UnitTest.Base
<hr></hr>

Ez a leírás a komponens **v1.0.0** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.0**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.0**
### A komponens összegyűjti a UnitTesteléssel kapcsolatos újrafelhazsnálható elemeinket/tudásunkat.

<hr></hr>

# Belefoglalt funkciók:
## VrhUnitTestBaseClass
A használathoz a **Vrh.UnitTest.Base névtér** using-olandó.
Ezt követően azokat a unit test classokat eből az osztályból származtatjuk, amelyekben szükségünk van aza alábbiak közül bármelyik képességre:

### 1. System.Reflection.Assembly.GetEntryAssembly() null javítás
A .Net-ben a System.Reflection.Assembly.GetEntryAssembly().Location segítségével nyerjük ki a munkakönyvtárt. Ez dióhéjban valahogy úgy működik, hogy az alkalmazástér ben beállítódik amikor a hostoló folyamat létrehozza a saját alkalmazás terét. A VS unit test framework-je azonban alacsony szintű reflexióval dolgozik, és az alkalmazás tért is így hozza létre. Ezért a System.Reflection.Assembly.GetEntryAssembly() hívás null-t ad Unit test környezetben. Így minden System.Reflection.Assembly.GetEntryAssembly().Location path beszerzés hibát (NullReferenceException) fog kiváltani. Ezt orvosolja az alkalmazás, azzal, hogy a konstruktorában beállítja a GetCallingAssembly-re az alkalmazástér EntryAssembly-jét.

Használata: Egyszerűen a VrhUnitTestBaseClass-ból származtatjuk a test class-t, ahol szükésg van ennek az értéknek a jelenlétére: 
```csharp
[TestClass]
public class VrhLoggerTests : VrhUnitTestBaseClass
{
    ...
}
```

A VrhUnitTestBaseClass ezen felül tartalmaz egy public static metódust, amely segítségével tetszőleges Assemblyt beállíthatunk Entry Assembly-ként az alaklmazástérben:
```csharp
/// <summary>
/// Megengedi, hogy beállítsd tetszőleges értékre az Entry Assembly-t az alkalmazástérben.
/// </summary>
/// <param name="assembly">Assembly amit entry assembly-nek akarsz beállítani</param>
public static void SetEntryAssembly(Assembly assembly)
```
Használata:
```csharp
VrhUnitTestBaseClass.SetEntryAssembly(assembly);
```

<hr></hr>

# Version History:

## V1.0.0 (2017.12.06)
### Initial version
