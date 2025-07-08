# encoding:utf-8
import requests 
import json
import ast

# 接口地址
url = "https://api.map.baidu.com/place/v3/region"

# 此处填写你在控制台-应用管理-创建应用后获取的AK
ak = "ZwZud9nuWcy3JB7olb2TLwB2syQPu13i"

params = {
    "query": "上海市闵行区新虹街道申昆路申虹国际大厦",
    "region": "上海",
    "output": "json",
    "is_light_version": False,
    "address_result": False,
    "ak": ak,
}

response = requests.get(url=url, params=params)
if response:
    data = json.loads(response.json())
    dataCleaned = data.replace("'", '"')
    # data = ast.literal_eval(response.json())
    firstResult = dataCleaned["results"][0]
    townValue = firstResult["town"]
    print(townValue)
    # print(response.json().results[0].town)