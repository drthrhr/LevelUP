#include<iostream>

//定义一个名称空间F，并在里面添加两个函数start stop
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

//我们也可以在std名称空间中添加类，虽然一般不这么做，这里仅仅用于帮助我们理解什么是名称空间
namespace std {
    struct Student {};
}

int main()
{
    F::start();//通过名称空间名称F访问该名称空间中的函数start,下同
    F::stop();

    G::start();
    G::stop();

    std::Student my;

    using namespace F;//将名称空间F中的名称暴露出来，不需要使用F::也可以访问
    start();//再次执行名称空间F中的start函数

    return 0;
}
