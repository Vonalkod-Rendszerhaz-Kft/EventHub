# Vrh.EventHub.Intervention
![](http://nuget.vonalkod.hu/content/projectavatars/EventHubIntervention.png)

*Ez a leírás a komponens **1.0.0** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.5**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5***

# Egyszerűen használható Intrervention interfész az EventHub fölé

## Korlátozások
* Csak Redis Pub/Sub csatornával működik
* Csak szinkron call implementációk lehetségesek
* Fenti pont miatt nem is támogat végponton belüli handlertúlterheléseket (Handler regisztráció létező csatornán, mindig felülírja az esetleg létező korábbi regisztrációt, a végpontban.)

## Az API
A komponens kommunikációra való használatához a **Vrh.EventHub.Intervention** nuget csomagot kell telepíteni. Jellemzően nem telepítjük külön ezt a csomagot, hanem valamelyik használni kívánt csatorna implemntáció függőségeként annak a Nuget csomagja "húzza le". 

A felhasználás helyén a **Vrh.EventHub.Intervention** névtért kell includolni:
```csharp
using Vrh.EventHub.Intervention;
```
Az API teljes funkcionalítása az EventHubIntervention static class public metódusain át érhető el. 

### Feldolgozó oldal
#### RegisterInterventionChannel - Handler regisztrációja
A fogadó végpontban ennek segítségével regisztrálunk handlereket (üzenetet fogadó, és azt feldolgozó metódusokat)
A metódus signatúrája az alábbi:
```csharp
        /// <summary>
        /// Registrál egy kezelő metódust az adott nevű csatornára érkező összes Beavatkozás számára
        ///     (Feldolgozó oldalon használjuk!)
        /// </summary>
        /// <param name="channelId">Csatorna azonosító</param>
        /// <param name="handler">A regisztrált handler (üzenetfogadó/feldolgozó metódus)</param>
        public static void RegisterInterventionChannel(string channelId, 
            Func<string, Dictionary<string, string>, Dictionary<string, string>> handler)
```

A kezelőként registrált metódus szignaturájéának meg kell fellelnie a
```csharp
Func<string, Dictionary<string, string>, Dictionary<string, string>>
```
definiciónak, amely egy az alábbi példának megfelelő metódust jelent:
```csharp
private Dictionary<string, string> SampleHandler(string interventionName, Dictionary<string, string> parameters)
```

Használata:
```csharp
EventHubIntervention.RegisterInterventionChannel("Sample Internvention Channel", InterventionHandler)
```
Alap minta az InterventionHandler implementációra:
```csharp
        private Dictionary<string, string> SampleInterventoionHandler(string interventionName, Dictionary<string, string> parameters)
        {
            var returnData = new Dictionary<string, string>();
            switch (interventionName)
            {
                case "iv1":
                    // process: do something!
                    returnData.Add("intervention", "iv1");
                    break;
                case "iv2":
                    // process: do something!
                    returnData.Add("intervention", "iv2");
                    break;
                default:
                    // Error
                    throw new Exception("1");
            }
            return returnData;
        }
```
Ahol:
* A bejovő stringre írt switch miatt megfontolandó, hogy a case-ek tényleges tevékenységét legalább elkülönült metódusokba szerevezzük!  
* A hibák visszajelzése a hivó oldalra a handlerben történő egyszerű Exception dobással lehetséges. Olyan Exception-ökert használjunk, amelyeket a hivó oldal is imer, különben problémás lezs a hivó oldalon a deserializáció! (Az intervention implementáció lényegi eleme az általánosság, ezért nincs a két oldal közti egyezmény, ahol a például a típusos hibakezelő rendszert terjeszteni lehetne!) 

#### DropInterventionChannel - Csatorna eldobása
Segítségével megszüntethetünk egy feldolgozó regisztrációt egy adott csatornárta a végpontban.

Használata:
```csharp
EventHubIntervention.DropInterventionChannel("Sample Internvention Channel")
```
### Hívó oldal
Beavatkozás hívása. A hívásra az API alábbi metódusa szolgál:
```csharp
/// <summary>
/// Beavatkozás meghívása
///     (Hívó oldalon használjuk!)
/// </summary>
/// <param name="channelId">A csatorna amire behívunk</param>
/// <param name="interventionName">A bevatkozás neve (azonosítója)</param>
/// <param name="parameters">A beavatkozás paraméterei</param>
/// <param name="timeout">Ebben a hívásban használni kívánt timeout (ha nincs megadva, akkor a csatorna szerinti default-ot használja)</param>
/// <returns>A beavatkozás által visszaadott adatok</returns>
public static Dictionary<string, string> RunIntervention(string channelId, 
                string interventionName, 
                Dictionary<string, string> parameters, 
                TimeSpan? timeout = null)
```
Használata:
```csharp
var parameters = new Dictionary<string, string>()
{
    { PARAMETER_NAME, PARAMETER_VALUE }
};
var resultData = EventHubIntervention.RunIntervention(channelName, 
                                                        INTERVENTION_NAME, 
                                                        parameters, 
                                                        new TimeSpan(0, 0, timeOutInSec));
```
Ahol: 
* resultData **Dictonary<string, string>** típus. A fogadó oldali implementációtól függően lehet null, vagy üres dictionary is.
* Az fogadó oldalon kiváltott Exception-ök a küldő oldalán exceptionként jelennek meg, fenti példa nem tartalmaz exception kezelést.
<hr></hr>

## Version History:
### 1.0.0 (2019.09.25)
- Initial version