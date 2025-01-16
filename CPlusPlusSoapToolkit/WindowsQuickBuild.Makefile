GSOAP=../gsoap/bin/soapcpp2
GWSDL=../gsoap/bin/wsdl2h
SOAPH=../gsoap/stdsoap2.h
SOAPCPP=../gsoap/stdsoap2.cpp

CC=gcc
CPP=g++
LIBS=-lcrypto -lssl -lws2_32 -lwininet
COFLAGS=-O2
CWFLAGS=-Wall
CIFLAGS=-I. -I../gsoap -I../gsoap/import -I../gsoap/plugin -I../gsoap/openssl -I../gsoap/custom
CMFLAGS=-DWITH_DOM -DWITH_OPENSSL -DGSOAP_WIN_WININET
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)


#Build the targets one by one in the below order.
#More steps, but reduces compilation and linking time if you're making changes to individual components.


header:	CyberSourceTransaction_1.224.wsdl
		$(GWSDL) -t ../gsoap/WS/WS-typemap.dat -s -o cybersource.h CyberSourceTransaction_1.224.wsdl
source:	
		$(GSOAP) -j -C -I../gsoap/import cybersource.h
wsseapi.o:	../gsoap/plugin/wsseapi.h ../gsoap/plugin/wsseapi.cpp
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/wsseapi.cpp
smdevp.o:	../gsoap/plugin/smdevp.h ../gsoap/plugin/smdevp.c
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/smdevp.c

soapITransactionProcessorProxy.o:	./soapITransactionProcessorProxy.h ./soapITransactionProcessorProxy.cpp
		$(CPP) $(CFLAGS) -c ./soapITransactionProcessorProxy.cpp

struct_timeval.o:	../gsoap/import/custom/struct_timeval.h ../gsoap/import/custom/struct_timeval.cpp
		$(CPP) $(CFLAGS) -c ../gsoap/import/custom/struct_timeval.cpp

stdsoap2.o:	./stdsoap2.h ./stdsoap2.cpp
		$(CPP) $(CFLAGS) -c ./stdsoap2.cpp

soapC.o:	./soapH.h ./soapC.cpp
		$(CPP) $(CFLAGS) -c ./soapC.cpp

gsoapWinInet.o:	../gsoap/import/gsoapWinInet.h ../gsoap/import/gsoapWinInet.cpp
		$(CPP) $(CFLAGS) -c ../gsoap/import/gsoapWinInet.cpp

wsaapi.o:	../gsoap/plugin/wsaapi.h ../gsoap/plugin/wsaapi.c
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/wsaapi.c

mecevp.o:	../gsoap/plugin/mecevp.h ../gsoap/plugin/mecevp.c
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/mecevp.c

threads.o:	../gsoap/plugin/threads.h ../gsoap/plugin/threads.c
		$(CPP) $(CFLAGS) -c ../gsoap/plugin/threads.c

binarySecurityTokenHandler.o:	BinarySecurityTokenHandler.h BinarySecurityTokenHandler.cpp
		$(CPP) $(CFLAGS) -c BinarySecurityTokenHandler.cpp 

propertiesUtil.o:	PropertiesUtil.h PropertiesUtil.cpp 
		$(CPP) $(CFLAGS) -c PropertiesUtil.cpp


cybsdemo:	sample.cpp $(SOAPH) $(SOAPCPP) ../gsoap/dom.cpp wsseapi.o smdevp.o
		$(CPP) $(CFLAGS) -o cybsdemo sample.cpp soapC.o ../gsoap/dom.cpp stdsoap2.o struct_timeval.o threads.o mecevp.o wsaapi.o wsseapi.o smdevp.o soapITransactionProcessorProxy.o gsoapWinInet.o propertiesUtil.o binarySecurityTokenHandler.o -L "C:/Program Files (x86)/Windows Kits/10/Lib/10.0.22000.0/um/x64" $(LIBS)