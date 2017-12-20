# bitfinex-websockets

Bitfinex WebSockets client

All that works so far is getting the ticker feed.  Contains a DLL and console app.

Usage: `dotnet run -- ticker BTCUSD LTC`

Output:

```
$ dotnet run -- ticker BTCUSD LTCUSD
Connecting...  connected.  Press a key to disconnect.
[opened]
[subscribed] BTCUSD
BTCUSD: 16276 vol: 89721.22657926 (b: 16271 <5> a: 16276)  -0.1273%
[subscribed] LTCUSD
LTCUSD: 317.67 vol: 589622.62319235 (b: 317.68 <0.28> a: 317.96)  -0.1148%
LTCUSD: 317.5 vol: 589706.94322114 (b: 317.47 <0.03> a: 317.5)  -0.1152%
BTCUSD: 16281 vol: 89721.67626418 (b: 16280 <13> a: 16293)  -0.1271%

[closed connection]

```


