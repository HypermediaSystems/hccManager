var sList = "abc";
var zStart = 13;
var zCnt = 3;
var yStart = 4281;
var yCnt = 4;
var xStart = 2705;
var xCnt = 4;

// 13/51.9639/8.2343
// https://b.tile.openstreetmap.org/13/4282/2706.png
var urlPattern = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";

for (var z = zStart; z < zStart + zCnt; z++) {
    for (var y = yStart; y < yStart + yCnt; y++) {
        for (var x = xStart; x < xStart + xCnt; x++) {
            var url = urlPattern.replace("{s}", sList[0])
                .replace("{z}", z.toString())
                .replace("{y}", y.toString())
                .replace("{x}", x.toString());
            console.log("get: " + url);
            window.JSBridge.AddCachedExternalData(url);
            for (var i = 1; i < sList.length; i++) {
                var aliasUrl = urlPattern.replace("{s}", sList[i])
                    .replace("{z}", z.toString())
                    .replace("{y}", y.toString())
                    .replace("{x}", x.toString());
                console.log("alias: " + url + " -> " + aliasUrl);
                window.JSBridge.AddCachedAlias(url,aliasUrl);
            }
        }
    }
    yStart *= 2;
    xStart *= 2;
    yCnt *= 2;
    xCnt *= 2;

}

