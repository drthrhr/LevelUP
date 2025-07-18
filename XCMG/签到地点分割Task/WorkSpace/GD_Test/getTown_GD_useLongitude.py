# encoding:utf-8
import requests 
import sys

reverse_geocoding_url = "https://restapi.amap.com/v3/geocode/regeo?parameters"

key_jz = "876433100914874df238e09c66dc022a"


if len(sys.argv) != 3:
    print('参数数目错误！应传参为： 【经度】 【纬度】')
    exit(1)


# longitudeParam = '117.201617' # 经度
# latitudeParam = '34.226415'  # 纬度
longitudeParam = sys.argv[1] # 经度
latitudeParam = sys.argv[2]  # 纬度


location = longitudeParam + ',' + latitudeParam

params = {
    "key": key_jz,
    "location": location,
    "output": "json"
}


response = requests.get(url=reverse_geocoding_url, params=params)
if response:
    dataDict = response.json()
    # print(dataDict)
    town = dataDict['regeocode']['addressComponent']['township']
    print(town, end='')