#adjust the below paths to point to your gsoap directory
GSOAP=../gsoap/bin/soapcpp2
GWSDL=../gsoap/bin/wsdl2h
SOAPH=../gsoap/stdsoap2.h
SOAPCPP=../gsoap/stdsoap2.cpp

CC=gcc
CPP=g++
LIBS=-lcrypto -lssl 
COFLAGS=-O2
CWFLAGS=-Wall
CIFLAGS=-I. -I../gsoap -I../gsoap/import -I../gsoap/plugin -I../gsoap/openssl -I../gsoap/custom
CMFLAGS=-DWITH_DOM -DWITH_OPENSSL -DGSOAP_WIN_WININET
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)


#Targets
#recommended build order: header -> source -> wsseapi.o -> smdevp.o -> cybsdemo


#cybsdemo target now includes many WSSE additional classes, as documented at:
#https://www.genivia.com/tutorials.html, scroll to "WS-Security authentication" section.

cybsdemo:	sample.cpp $(SOAPH) $(SOAPCPP) ../gsoap/dom.cpp wsseapi.o smdevp.o
		$(CPP) $(CFLAGS) -o cybsdemo sample.cpp soapC.cpp ../gsoap/dom.cpp stdsoap2.cpp ../gsoap/import/custom/struct_timeval.cpp ../gsoap/plugin/threads.c ../gsoap/plugin/mecevp.c ../gsoap/plugin/wsaapi.c wsseapi.o smdevp.o soapITransactionProcessorProxy.cpp ../gsoap/import/gsoapWinInet.cpp PropertiesUtil.cpp BinarySecurityTokenHandler.cpp $(LIBS)


header:	CyberSourceTransaction_1.224.wsdl
		$(GWSDL) -t ../gsoap/WS/WS-typemap.dat -s -o cybersource.h CyberSourceTransaction_1.224.wsdl
source:	
		$(GSOAP) -j -C -I../gsoap/import cybersource.h
wsseapi.o:	../gsoap/plugin/wsseapi.h ../gsoap/plugin/wsseapi.cpp
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/wsseapi.cpp
smdevp.o:	../gsoap/plugin/smdevp.h ../gsoap/plugin/smdevp.c
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/smdevp.c

clean:
		rm -f *.o soapH.h soapStub.h soapC.cpp soapClient.cpp soap*Proxy.h soap*Object.h soapClientLib.cpp
distclean:
		rm -f *.o *.xml *.nsmap *.log soapH.h soapStub.h soapC.cpp soapClient.cpp soapClientLib.cpp soap*Proxy.h soap*Object.h cybsdemo cybersource.h
