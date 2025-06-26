#include <iostream>
#include <string>
using namespace std;

int main()
{
	cout << "Hello CMake." << endl;

	string line1, line2;

	/* 输入:111\n222 （\n代表回车） */
	//getline(cin, line1);	// line1 = "111"
	//cin >> line2;	// line2 = "222"

	/* 输入:111\n222 （\n代表回车）（实际上输入完111\n就执行完这两条语句了） */
	//cin >> line1;	// line1 = "111"
	//getline(cin, line2);	// line2 = ""

	/* 输入: \n\n  \n111\n\n\n  222 \n （\n代表回车） */
	//cin >> line1 >> line2;	// line1 = "111", line2 = "222"
	
	/* 输入：\t  \t   111  \t\n\n222（\t代表tab；\n代表回车）*/
	getline(cin, line1);	// line1 = "\t  \t   111  \t"
	getline(cin, line2);	// line2 = ""

	return 0;
}
