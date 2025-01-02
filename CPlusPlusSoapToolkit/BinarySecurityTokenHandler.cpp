//GENAI=YES

#include "soapH.h"
#include "wsseapi.h"
#include <openssl/pkcs12.h>
#include <openssl/err.h>

#include "PropertiesUtil.h"

using namespace std;

#ifdef WIN32
static HANDLE* lock_cs;
#endif /* WIN32 */


//Adapted from NVPClient.cpp

#ifdef WIN32
void cybs_openssl_init(void)
{
    int i;
    OpenSSL_add_all_algorithms();
    //ERR_load_crypto_strings();
    lock_cs = (HANDLE*)OPENSSL_malloc(CRYPTO_num_locks() * sizeof(HANDLE));
    for (i = 0; i < CRYPTO_num_locks(); i++)
    {
        lock_cs[i] = CreateMutex(NULL, FALSE, NULL);
    }

    CRYPTO_set_locking_callback((void (*)(int, int, const char*, int))win32_locking_callback);
    /* id callback defined */
}

void cybs_openssl_cleanup(void)
{
    int i;

    CRYPTO_set_locking_callback(NULL);
    for (i = 0; i < CRYPTO_num_locks(); i++)
        CloseHandle(lock_cs[i]);
    OPENSSL_free(lock_cs);
}

void win32_locking_callback(int mode, int type, const char* file, int line)
{
    if (mode & CRYPTO_LOCK)
    {
        WaitForSingleObject(lock_cs[type], INFINITE);
    }
    else
    {
        ReleaseMutex(lock_cs[type]);
    }
}
#endif /* WIN32 */

#ifdef PTHREADS

static pthread_mutex_t* lock_cs;
static long* lock_count;

void cybs_openssl_init(void)
{
    int i;

    lock_cs = (pthread_mutex_t*)OPENSSL_malloc(CRYPTO_num_locks() * sizeof(pthread_mutex_t));
    lock_count = (long*)OPENSSL_malloc(CRYPTO_num_locks() * sizeof(long));
    for (i = 0; i < CRYPTO_num_locks(); i++)
    {
        lock_count[i] = 0;
        pthread_mutex_init(&(lock_cs[i]), NULL);
    }

    CRYPTO_set_id_callback((unsigned long (*)())pthreads_thread_id);
    /* CRYPTO_set_locking_callback((void (*)())pthreads_locking_callback); */
    CRYPTO_set_locking_callback((void (*)(int, int, const char*, int))pthreads_locking_callback);
}

void cybs_openssl_cleanup(void)
{
    int i;

    CRYPTO_set_locking_callback(NULL);
    /* fprintf(stderr,"cleanup\n"); */
    for (i = 0; i < CRYPTO_num_locks(); i++)
    {
        pthread_mutex_destroy(&(lock_cs[i]));
        /* fprintf(stderr,"%8ld:%s\n",lock_count[i],
            CRYPTO_get_lock_name(i)); */
    }
    OPENSSL_free(lock_cs);
    OPENSSL_free(lock_count);

    /* fprintf(stderr,"done cleanup\n"); */
}

void pthreads_locking_callback(int mode, int type, const char* file,
    int line)
{
#ifdef undef
    fprintf(stderr, "thread=%4d mode=%s lock=%s %s:%d\n",
        CRYPTO_thread_id(),
        (mode & CRYPTO_LOCK) ? "l" : "u",
        (type & CRYPTO_READ) ? "r" : "w", file, line);
#endif

    if (mode & CRYPTO_LOCK)
    {
        pthread_mutex_lock(&(lock_cs[type]));
        lock_count[type]++;
    }
    else
    {
        pthread_mutex_unlock(&(lock_cs[type]));
    }
}

unsigned long pthreads_thread_id(void)
{
    unsigned long ret;

    ret = (unsigned long)pthread_self();
    return(ret);
}

unsigned long cybs_get_thread_id(void)
{
    return(pthreads_thread_id());
}

#endif /* PTHREADS */

class CYBSCPP_BEGIN_END
{
public:
    CYBSCPP_BEGIN_END()
    {
        /* This is a place for any one-time initializations needed by
           the client. */
        cybs_openssl_init();
    }

    ~CYBSCPP_BEGIN_END()
    {
        /* This is a place for any one-time cleanup tasks. */
        cybs_openssl_cleanup();
        ERR_free_strings();
        EVP_cleanup();
        CRYPTO_cleanup_all_ex_data();

    }
};

CYBSCPP_BEGIN_END gCybsBeginEnd;


EVP_PKEY* pkey1 = NULL;
X509* cert1 = NULL;
STACK_OF(X509)* ca = NULL;

//based on NVPClient.configure()
int addSecurityTokenAndSignature(struct soap* soap)
{
    loadProperties();

    soap_ssl_init();

    soap_register_plugin(soap, soap_wsse);

    //Read pkcs12
    BIO* bio1;
    bio1 = BIO_new_file(getKeyFilePath(), "rb");

    if (!bio1) {
        std::cerr << "Error opening file: " << getKeyFilePath() << std::endl;
        return (1);
    }

    PKCS12* p12 = d2i_PKCS12_bio(bio1, NULL);

    BIO_free(bio1);

    if (!p12) {
        ERR_print_errors_fp(stderr);
        return (1);
    }

    pkey1 = NULL;
    cert1 = NULL;
    ca = NULL;
    const char* password = getProperty("KEY_PASS");

    if (!PKCS12_parse(p12, password, &pkey1, &cert1, &ca)) {
        ERR_print_errors_fp(stderr);
        return (2);
    }

    PKCS12_free(p12);
    //Read pkcs12 completed

    soap_set_omode(soap, SOAP_XML_CANONICAL);

    //Set up configuration for signing the request
    soap_wsse_set_wsu_id(soap, "wsse:BinarySecurityToken SOAP-ENV:Body");

    if (soap_wsse_add_BinarySecurityTokenX509(soap, "X509Token", cert1)
        || soap_wsse_add_KeyInfo_SecurityTokenReferenceX509(soap, "#X509Token")
        || soap_wsse_sign_body(soap, SOAP_SMD_SIGN_RSA_SHA256, pkey1, 0)
        || soap_wsse_sign_only(soap, "Body")) {
        std::cerr << "Error adding BinarySecurityToken or Signature" << std::endl;
        return (3);
    }

    return (0);
}

//based on NVPClient.opensslCleanup()
void cleanupSecurityTokenAndSignature() {
    sk_X509_pop_free(ca, X509_free);
    X509_free(cert1);
    EVP_PKEY_free(pkey1);
}
