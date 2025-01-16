//GENAI=YES

#ifndef BINARYSECURITYTOKENHANDLER_H
#define BINARYSECURITYTOKENHANDLER_H

#include <openssl/x509.h>

// Function declarations
int addSecurityTokenAndSignature(struct soap* soap);
void cleanupSecurityTokenAndSignature();

#endif // BINARYSECURITYTOKENHANDLER_H
