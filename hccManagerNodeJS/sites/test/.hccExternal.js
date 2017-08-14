function externals(args){
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
	return JSPN.stringify(ret);
}