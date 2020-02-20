GSOAP=../gsoap/bin/soapcpp2
GWSDL=../gsoap/bin/wsdl2h
SOAPH=../gsoap/stdsoap2.h
SOAPCPP=../gsoap/stdsoap2.cpp
CC=gcc
CPP=g++
LIBS=-lcrypto -lssl
COFLAGS=-O2
CWFLAGS=-Wall
CIFLAGS=-I. -I../gsoap -I../gsoap/plugin
CMFLAGS=-DWITH_DOM -DWITH_OPENSSL
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)
cybsdemo:	sample.cpp $(SOAPH) $(SOAPCPP) ../gsoap/dom.cpp wsseapi.o smdevp.o
		$(CPP) $(CFLAGS) -o cybsdemo sample.cpp soapC.cpp soapClient.cpp ../gsoap/dom.cpp $(SOAPCPP) wsseapi.o smdevp.o $(LIBS)
header:	CyberSourceTransaction.wsdl
		$(GWSDL) -t ../gsoap/WS/WS-typemap.dat -s -o cybersource.h CyberSourceTransaction.wsdl
source:	
		$(GSOAP) -C -I../gsoap/import cybersource.h
wsseapi.o:	../gsoap/plugin/wsseapi.h ../gsoap/plugin/wsseapi.c
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/wsseapi.c
smdevp.o:	../gsoap/plugin/smdevp.h ../gsoap/plugin/smdevp.c
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/smdevp.c
clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapClient.cpp soap*Proxy.h soap*Object.h soapClientLib.cpp
distclean:
		rm -f *.o *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapClient.cpp soapClientLib.cpp soap*Proxy.h soap*Object.h cybsdemo cybersource.h
