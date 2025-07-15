import pandas as pd
import wordninja
import re
import os
import json

######  生成UI对比项目的公共代码。
######  在下方指定需要处理的Excel文件、sheet列表、输出目录等信息。
######  扫描所指定表格中 "编号" 列，获取用例编号，生成对应测试脚本文件、对应测试页面文件、更新 pages.json 文件中有关内容、更新 List.test.ets 文件中有关内容。

# 设置是否覆盖已存在的文件
OVERRIDE_FLAG = False   # 如果设置为 True，则会覆盖已存在的页面文件和测试脚本文件；如果设置为 False，则会跳过已存在的文件
# OVERRIDE_FLAG = True

# 设置需要处理的Excel文件
FILE_NAME = "用例信息.xlsx"

# 设置需要处理的sheet列表
SHEET_NAMES = [
    "用例明细"
]

# 设置wordninja自定义词库路径
WORDNINJA_MODLE = 'D:\\Learning\\Tools\\UICompare\\wordninja_words.txt.gz'

# 设置输出目录
# TEST_OUTPUT_DIR = "D:\\Learning\\Tools\\UICompare\\ets\\test\\"  # 测试脚本输出目录
# PAGES_DIR = "D:\\Learning\\Tools\\UICompare\\ets\\testability\\pages\\"  # 用例页面输出目录
# PAGES_JSON_FILE = "D:\\Learning\\Tools\\UICompare\\test_pages.json"  # 指定要修改的 pages.json 文件路径
# LIST_TEST_ETS_FILE = "D:\\Learning\\Tools\\UICompare\\ets\\test\\List.test.ets"  # List.test.ets 文件路径
TEST_OUTPUT_DIR = "D:\\MarkedCodes\\uicompare_250521begin\\uicompare\\entry\\src\\ohosTest\\ets\\test\\"  # 测试脚本输出目录
PAGES_DIR = "D:\\MarkedCodes\\uicompare_250521begin\\uicompare\\entry\\src\\ohosTest\\ets\\testability\\pages\\"  # 用例页面输出目录
PAGES_JSON_FILE = "D:\\MarkedCodes\\uicompare_250521begin\\uicompare\\entry\\src\\ohosTest\\resources\\base\\profile\\test_pages.json"  # 指定要修改的 pages.json 文件路径
LIST_TEST_ETS_FILE = "D:\\MarkedCodes\\uicompare_250521begin\\uicompare\\entry\\src\\ohosTest\\ets\\test\\List.test.ets"  # List.test.ets 文件路径


# 页面模板字符串
# 这里的 [page_name_replace] 会被实际的页面名称替换
PAGE_TEMPLATE = """
/*
 * Copyright (c) 2025 Shenzhen Kaihong Digital Industry Development Co., Ltd.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

@Entry
@Component
struct [page_name_replace] {
  @State name: string = '[page_name_replace]';
  @State message: string = this.name;

  build() {
    Column() {
      Text(this.message)
        .fontSize(20)
        .fontColor(Color.Black)
        .fontWeight(FontWeight.Bold)
        .textAlign(TextAlign.Center)
        .margin({
          left: 6,
          right: 6,
          top: 10,
          bottom: 10
        })
      
      
      
    }
    .justifyContent(FlexAlign.Center)
    .width('100%')
    .height('100%')
  }
}
"""

# 测试脚本模板字符串
# 这里的 [suite_name_replace] 会被实际 suite 名称替换； [cases_template_replace] 会被具体的各条用例内容替换
TEST_TEMPLATE = """
/**
 * Copyright (c) 2025 Shenzhen Kaihong Digital Industry Development Co., Ltd.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

import { describe, beforeAll, beforeEach, afterEach, afterAll, it, expect, Level } from '@ohos/hypium';
import { BusinessError, commonEventManager } from '@kit.BasicServicesKit';
import { Driver, ON ,Component, UiWindow } from '@kit.TestKit';
import { uiAppearance } from '@kit.ArkUI';
import Settings from '../model/Settings';
import windowSnap from '../model/snapShot';
import Logger from '../model/Logger';
import Utils from '../model/Utils';


let TAG = 'RunTimeTest';

function sleep(ms: number) {
  return new Promise<string>(resolve => setTimeout(resolve, ms));
}

export default function [suite_name_replace]() {
  describe('[suite_name_replace]', () => {
    beforeAll(() => {

    })
    beforeEach(() => {

    })

    afterEach(async (done: Function) => {
      if (Settings.windowClass == undefined) {
        return
      }
      Settings.windowClass.destroyWindow((err) => {
        if (err.code) {
          Logger.error(`[${TAG}_afterEach]`, `Failed to destroy the window. Cause : ${JSON.stringify(err)}`);
          return;
        }
        Logger.info(`[${TAG}_afterEach]`, `Succeeded in destroy the window.`);
      })
      await Utils.sleep(1000);
      done();
    })

    afterAll(() => {

    })

    [cases_template_replace]

  })
}
"""

# 测试脚本模板字符串
# 这里的 [case_id_replace] 会被实际的 caseID 替换；[case_description_replace] 会被实际的 case 描述替换；[page_name_replace] 会被实际的页面名称替换；[suite_name_replace] 会被实际 suite 名称替换
CASE_TEMPLATE = """
    /*
     * @tc.number  [case_id_replace]
     * @tc.name    [case_id_replace]
     * @tc.desc    [case_description_replace]
     * @tc.level   3
     */
    it('[case_id_replace]', Level.LEVEL3, async (done: Function) => {
      TAG = '[case_id_replace]';
      let pageName = "[page_name_replace]";
      Logger.info(`[${TAG}]`, `Case start.`);
      Settings.createWindow('testability/pages/[suite_name_replace]/' + pageName);
      let driver: Driver = Driver.create();
      let button: Component = await driver.waitForComponent(ON.id(pageName + '_01'), 1000);
      await driver.waitForIdle(500, 1000);
      await windowSnap.snapShot();
      await Utils.sleep(1000);
      Logger.info(`[${TAG}]`, `Case finish.`);
      done();
    })
"""


# ------ 读取Excel文件，获取 suiteName 、 pageNameList 、 caseIDList ------
excel_file = pd.ExcelFile(FILE_NAME)

for sheet_name in SHEET_NAMES:
  if sheet_name not in excel_file.sheet_names:
      print(f"警告: Sheet {sheet_name} 不存在，跳过处理")
      continue               

  caseList = [] # 列表，用于存储所有用例经处理后，各用例所对应的 suiteName 、 pageName 和 caseID 信息
  suiteName = ''  # 各用例所属的测试套件名
  suiteNameList = []  # 用于存储所有用例所涉及到的测试套件名

  df = pd.read_excel(excel_file, sheet_name=sheet_name) # 读取Excel文件中指定的sheet 
  for caseID in df["编号"]:
    processStr = caseID
    prefix1 = 'SUB_'
    prefix2 = 'ACE_'

    # 依序移除前缀字符串 prefix1 、 prefix2
    if processStr.startswith(prefix1):
      processStr = processStr[len(prefix1):]
    if processStr.startswith(prefix2):
      processStr = processStr[len(prefix2):]

    # 获取末尾数字，之后移除末尾数字和下划线
    numberList = re.findall(r'\d+$', processStr)   # 获取 processStr 中的数字，返回列表。取列表第0项，得到case末尾的数字
    caseNum = numberList[0]
    processStr = re.sub(r'\d+$', '', processStr)   # 移除后缀的数字，如‘0010’
    processStr = processStr.replace('_', '')    # 移除所有的下划线  

    # 分词，并获得大驼峰形式的 suiteName 、pageName
    # words = wordninja.split(processStr)
    lm = wordninja.LanguageModel(WORDNINJA_MODLE)
    words = lm.split(processStr)
    suiteName = ''
    for word in words:
      if word == 'UI':
        suiteName = suiteName + word
        continue
      suiteName = suiteName + word.title()
    pageName = suiteName + caseNum      

    caseList.append({'suiteName': suiteName, 'pageName': pageName, 'caseID': caseID})
    if suiteName not in suiteNameList:
      suiteNameList.append(suiteName)

  caseList.sort(key=lambda x: x['caseID'])  # 对所有 case，按照 caseID 排序
  suiteNameList.sort()  # 对所有 suiteName 排序


  # ------ 依据所获取到的 suiteName 、 pageNameList 、 caseIDList ，生成各文件 ------

  for suiteName in suiteNameList: # 遍历所有 suiteName
    print(f"\n\n\n\n======处理测试套件: {suiteName}======")      
    
    # --- 生成页面文件，并更新 pages.json 文件
    for case in caseList:
      if case['suiteName'] != suiteName:  # 如果当前 case 的 suiteName 不等于正在处理的 suiteName，则跳过
        continue
      print(f"\n\n---处理用例: {case['caseID']}---")

      pageName = case['pageName']
      caseID = case['caseID']

      # 生成页面文件
      page_dir = PAGES_DIR + f"{suiteName}\\"
      page_content = PAGE_TEMPLATE.replace("[page_name_replace]", pageName)
      os.makedirs(os.path.dirname(page_dir), exist_ok=True)    # 确保目录存在
      output_page_file = page_dir + f"{pageName}.ets"    

      if OVERRIDE_FLAG:
        print(f"覆盖页面文件: {output_page_file}")
        with open(output_page_file, 'w', encoding='utf-8') as f:
          f.write(page_content)
      else:
        if not os.path.exists(output_page_file):    # 检查文件是否存在，如果不存在则创建
          print(f"创建页面文件: {output_page_file}")
          with open(output_page_file, 'w', encoding='utf-8') as f:
              f.write(page_content)
        else:
          print(f"页面文件已存在: {output_page_file}，跳过创建")

      # 更新 pages.json 文件
      if os.path.exists(PAGES_JSON_FILE):
        with open(PAGES_JSON_FILE, 'r') as jsonFile:
          content = json.load(jsonFile)

          # 检查 pages.json 中是否已存在该页面
          if 'src' not in content:
            content['src'] = []
          if f'testability/pages/{suiteName}/{pageName}.ets' not in content['src']:
            print(f"添加页面到 pages.json: {suiteName}/{pageName}.ets")
            content['src'].append('testability/pages/' + suiteName + '/' + pageName) # 添加新的页面路径到 src 列表中
            with open(PAGES_JSON_FILE, 'w') as jsonFile:
              json.dump(content, jsonFile, indent=2, ensure_ascii=False)
          else:
            print(f"pages.json 中已存在:  testability/pages/{suiteName}/{pageName}.ets ，跳过更新")  
          

    # --- 生成测试脚本文件
    test_dir = TEST_OUTPUT_DIR + f"{suiteName}Test\\"
    test_content = TEST_TEMPLATE.replace("[suite_name_replace]", suiteName)
    cases_content = ""

    # 先生成各条测试用例
    for case in caseList:
      if case['suiteName'] != suiteName:  # 如果当前 case 的 suiteName 不等于正在处理的 suiteName，则跳过
        continue

      # 生成各条测试用例，组合为 cases_content
      case_content = CASE_TEMPLATE
      case_content = case_content.replace("[case_id_replace]", case['caseID'])
      case_content = case_content.replace("[page_name_replace]", case['pageName'])
      case_content = case_content.replace("[case_description_replace]", "")  # 暂时设为空，到时候手动添加经翻译过的 case 描述
      case_content = case_content.replace("[suite_name_replace]", suiteName)
      cases_content += case_content

    # 将组合出的 cases_content 替换到测试脚本中
    test_content = test_content.replace("[cases_template_replace]", cases_content)

    # 生成测试脚本 .test.ets 文件
    os.makedirs(os.path.dirname(test_dir), exist_ok=True)    # 确保目录存在
    output_test_file = test_dir + f"{suiteName}.test.ets"

    if OVERRIDE_FLAG:
      print(f"\n覆盖测试脚本文件: {output_test_file}")
      with open(output_test_file, 'w', encoding='utf-8') as f:
        f.write(test_content)
    else:
      if not os.path.exists(output_test_file):    # 检查文件是否存在，如果不存在则创建
        print(f"\n创建测试脚本文件: {output_test_file}")
        with open(output_test_file, 'w', encoding='utf-8') as f:
            f.write(test_content)
      else:
        print(f"\n测试脚本文件已存在: {output_test_file}，跳过创建")
    

    # --- 更新 List.test.ets 文件
    if os.path.exists(LIST_TEST_ETS_FILE):
      with open(LIST_TEST_ETS_FILE, 'r', encoding='utf-8') as f:
        list_content = f.read()

      # List.test.ets 文件中新增import语句、调用测试套件语句
      if f"{suiteName}();" not in list_content: 

        # 在最后的 (); 之后插入 调用测试套件 语句
        func_statement = f"\n  {suiteName}();"
        func_insert_index = list_content.rfind('();') + 3  
        list_content = list_content[:func_insert_index] + func_statement + list_content[func_insert_index:]

        # 在最前面的 import 语句之前插入 import 语句
        import_statement = f"import {suiteName}\n  from './{suiteName}Test/{suiteName}.test';\n"
        import_insert_index = list_content.find('import')  # 在第一个 import 语句前插入新的 import 语句
        list_content = list_content[:import_insert_index] + import_statement + list_content[import_insert_index:]      

        # 写入文件
        with open(LIST_TEST_ETS_FILE, 'w', encoding='utf-8') as f:
            f.write(list_content)
        print(f"\n更新 List.test.ets 文件: {LIST_TEST_ETS_FILE}")
        
      else:
        print(f"\n测试套件 {suiteName} 已存在于 List.test.ets 文件中，跳过更新")
