# Introduction

hccManger is a Xamarin Forms application that can create SqLite databases to be used with [hccPlayer](../../../hccPlayer).

hccManager reads a .hhcConfig file from the server.

Suppose the server url is `https://www.test.org`

# .hccConfig

## A basic file

    {
        "url": "http://www.leaflon.test",
        "alias": "http___www_leaflon_test_",
        "zipped": "1",
        "defaultUrl"."index.html",
        "files": [
               "index.html"
        ],
    }`

This will request the file `https://www.test.org/index.html` add store it zipped in the database. The url or id will be set to `http://www.leaflon.test/index.html`.

`index.html` will be stored as defaultUrl in the metadata table.

## With filelist

    {
        "url": "http://www.leaflon.test",
        "alias": "http___www_leaflon_test_",
        "zipped": "1",
        "defaultUrl"."index.html",
        "fileList": ".hccFilelist.json"
    }`


.hccFilelist.json

    `[
      "ts/app.js",
      "ts/map.js",
      "ts/Leaflon/flagmesh.js",
      "ts/Leaflon/gpx.js",
      "ts/Leaflon/map.js",
      "ts/Leaflon/provider.js",
      "ts/Leaflon/providerlist.js",
      "ts/Leaflon/rest.js",
      "ts/Leaflon/subtile.js",
      "ts/Leaflon/tile.js",
      "ts/Leaflon/tilemesh.js",
      "Scripts/Babylon/babylon.js",
      "Scripts/Babylon/babylon.max.js",
      "Scripts/Babylon/cannon.js",
      "Scripts/Babylon/hand.js",
      "Scripts/Babylon/Oimo.js",
      "Scripts/Babylon/pep.js",
      "css/app.css",
      "index.html"
    ]`

hccManager will request the file `https://www.test.org/.hccFilelist.json` and loop over the array. Each file, e.g. `https://www.test.org/ts/app.js` will be stored zipped in the database. The url or id will be set to `http://www.leaflon.test/ts/app.js`.

The file .hccFilelist.json can be created with a gulp job:

    `GULP JOB`


## With external data

    `{
        "url": "http://www.leaflon.test",
        "alias": "http___www_leaflon_test_",
        "zipped": "1",
        "defaultUrl"."index.html",
        "fileList": ".hccFilelist.json"
        "externalUrl": [
          { "url": "http://a.tile.openstreetmap.org/13/4282/2706.png" },
          { "url": "http://openelevationmap.org/tile/13/4282/2706.jpg" }
          
        ]
    }`

hccManager will additionllay request the 2 external resources. e.g `http://a.tile.openstreetmap.org/13/4282/2706.png`. The url or id will be set to the original url `http://a.tile.openstreetmap.org/13/4282/2706.png`

## With dynamically determined external data


    `{
        "url": "http://www.leaflon.test",
        "alias": "http___www_leaflon_test_",
        "zipped": "1",
        "defaultUrl"."index.html",
        "fileList": ".hccFilelist.json"
        "externalJS": {
          "js": ".hccExternal.js"
        }
    }`


.hccExternal.js

    `
    function getOne(urlPattern, sList) {
        for (var z = zStart; z < zStart + zCnt; z++) {
            for (var y = yStart; y < yStart + yCnt; y++) {
                for (var x = xStart; x < xStart + xCnt; x++) {
                    var url = urlPattern.replace("{s}", sList[0])
                        .replace("{z}", z.toString())
                        .replace("{y}", y.toString())
                        .replace("{x}", x.toString());
                    **external.AddCachedExternalData**(url);
                    for (var i = 1; i < sList.length; i++) {
                        var aliasUrl = urlPattern.replace("{s}", sList[i])
                            .replace("{z}", z.toString())
                            .replace("{y}", y.toString())
                            .replace("{x}", x.toString());
                        **external.AddCachedAlias**(aliasUrl, url);
                    }
                }
            }
            yStart *= 2;
            xStart *= 2;
            yCnt *= 2;
            xCnt *= 2;
        
        }
    }
    
    
    
    var zStart = 11;
    var zCnt = 1;
    var xStart = 1514;
    var xCnt = 9;
    var yStart = 854;
    var yCnt = 9;
    
    getOne("http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", "abc");
    
    zStart = 11;
    zCnt = 1;
    xStart = 1514;
    xCnt = 9;
    yStart = 854;
    yCnt = 9;
    
    getOne("http://openelevationmap.org/tile/{z}/{x}/{y}.jpg", ".");
    `

hccManager will request the file `https://www.test.org/.hccExternal.js` and execute it. This file is plain JavaScript with 2 additional functions:

`external.AddCachedAlias(aliasUrl, url);` will add an entry to the alias table,

and 

`external.AddCachedExternalData(url)` will request the data from the given url and add it to the database.

fff


    `{
        "url": "http://www.leaflon.test",
        "alias": "http___www_leaflon_test_",
        "zipped": "1",
        "defaultUrl"."index.html",
        "fileList": ".hccFilelist.json"
        "externalUrl": [
          { "url": "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" },
          { "url": "http://openelevationmap.org/tile/{z}/{x}/{y}.jpg" }
          
        ],
        "externalJS": {
          "js": ".hccExternal.js",
          "vars": [
            {
              "name": "zMin",
              "desc": "min Zoom level"
            },
            {
              "name": "zCnt",
              "desc": "count of Zoom levels"
            },
            {
              "name": "xMin",
              "desc": "min value of x at min zoom level"
            },
            {
              "name": "xCnt",
              "desc": "count of x at min zoom level"
            },
            {
              "name": "yMin",
              "desc": "min value of y at min zoom level"
            },
            {
              "name": "yCnt",
              "desc": "count of y at min zoom level"
            }
          ]
        }
    }`



