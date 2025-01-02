//GENAI=YES

#ifndef PROPERTIESUTIL_H
#define PROPERTIESUTIL_H


#include <string>
#include <map>

// Constants
extern const std::string PROPERTIES_PATH;
extern std::map<std::string, std::string> merchantProperties;

// Function declarations
std::map<std::string, std::string> loadProperties();
char* getKeyFilePath();
char* getProperty(std::string key);

#endif // PROPERTIESUTIL_H
