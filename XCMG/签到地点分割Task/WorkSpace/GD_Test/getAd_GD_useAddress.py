import requests 
import sys

key_jz = "876433100914874df238e09c66dc022a"

search_poi_url = "https://restapi.amap.com/v5/place/text?parameters"
reverse_geocoding_url = "https://restapi.amap.com/v3/geocode/regeo?parameters"

if len(sys.argv) != 3:
    print('参数数目错误！应传参为： 【查询区域（市）】 【查询关键词】')
    exit(1)

regionParam = sys.argv[1]
queryParam = sys.argv[2]

search_poi_params = {
    "key": key_jz,
    "keywords": queryParam,
    "region": regionParam
}

location = ''
response = requests.get(search_poi_url, params=search_poi_params)
if response:
    dataDict = response.json()
    # print(dataDict)
    adName = dataDict['pois'][0]['adname']
    print(adName)
    # params = {
    # "key": key_jz,
    # "location": location,
    # "output": "json"
    # }
    # response = requests.get(url=reverse_geocoding_url, params=params)
    # if response:
    #     dataDict = response.json()
    #     # print(dataDict)
    #     town = dataDict['regeocode']['addressComponent']['township']
    #     print(town, end='')


