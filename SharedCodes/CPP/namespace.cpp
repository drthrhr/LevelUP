#include<iostream>

//����һ�����ƿռ�F���������������������start stop
namespace F {
    void start() {
		std::cout << "F::start called" << std::endl;
    }
    void stop() {
		std::cout << "F::stop called" << std::endl;
    }
}
namespace G {
    void start() {
		std::cout << "G::start called" << std::endl;
    }
    void stop() {
		std::cout << "G::stop called" << std::endl;
    }
}

//����Ҳ������std���ƿռ�������࣬��Ȼһ�㲻��ô��������������ڰ����������ʲô�����ƿռ�
namespace std {
    struct Student {};
}

int main()
{
    F::start();//ͨ�����ƿռ�����F���ʸ����ƿռ��еĺ���start,��ͬ
    F::stop();

    G::start();
    G::stop();

    std::Student my;

    using namespace F;//�����ƿռ�F�е����Ʊ�¶����������Ҫʹ��F::Ҳ���Է���
    start();//�ٴ�ִ�����ƿռ�F�е�start����

    return 0;
}
