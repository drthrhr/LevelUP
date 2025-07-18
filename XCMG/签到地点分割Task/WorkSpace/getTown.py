# encoding:utf-8
import requests 
import sys

# 接口地址
url = "https://api.map.baidu.com/place/v3/region"

# 此处填写你在控制台-应用管理-创建应用后获取的AK
ak_jz = "ZwZud9nuWcy3JB7olb2TLwB2syQPu13i"
ak_djn = "rCM17fD0muJdHmuzgBe6e6AATfXhRTrZ"

# 调用该Python脚本时传入参数： xx市 xxxxxx(地址信息)
if len(sys.argv) != 3:
    print('参数数目错误！应传参为： 【城市名】 【所查询地址信息】')
    exit(1)

regionParam = sys.argv[1]   # 获取xx市的参数
queryParam = sys.argv[2]    # 获取地址信息的参数
# print('regionParam: ' + regionParam)
# print('queryParam: ' + queryParam)

params = {
    "query": queryParam,
    "region": regionParam,
    "output": "json",
    "is_light_version": "false",
    "address_result": "false",
    "scope": "2",
    "ak": ak_jz,
}

response = requests.get(url=url, params=params)
if response:
    dataDict = response.json()
    # print(dataDict)
    if dataDict['status'] == 302:   # 原ak超限了，使用新的ak重新拼装参数，并重新请求
        params = {
            "query": queryParam,
            "region": regionParam,
            "output": "json",
            "is_light_version": "false",
            "address_result": "false",
            "scope": "2",
            "ak": ak_djn,
        }
        response = requests.get(url=url, params=params)
        if response:   
            dataDict = response.json()
            # print(dataDict)
            town = dataDict['results'][0]['town']
            print(town, end='')
    else:
        town = dataDict['results'][0]['town']
        print(town, end='')