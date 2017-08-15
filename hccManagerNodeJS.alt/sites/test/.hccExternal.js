function externals(args) {
    args.zMin
    args.zCnt
    args.xMin
    args.xCnt
    args.yMin
    args.yCnt
    
    var ret = [];
    ret.push(
            {url=["https://www.extern.de/data.json"]}
);



// 13/51.9639/8.2343
// https://b.tile.openstreetmap.org/13/4282/2706.png
var urlPattern = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";

for (var z = args.zMin; z < args.zMin + args.zCnt; z++) {
    for (var y = args.yMin; y < args.yMin + args.yCnt; y++) {
        for (var x = args.xMin; x < args.xMin + args.xCnt; x++) {
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
                window.JSBridge.AddCachedAlias(url, aliasUrl);
            }
        }
    }
    args.yMin *= 2;
    args.xMin *= 2;
    args.yCnt *= 2;
    args.xCnt *= 2;

}



return JSPN.stringify(ret);
}
