//GENAI=YES

#include <iostream>
#include <fstream>
#include <string>
#include <cstring>
#include <map>

#include "PropertiesUtil.h"


const std::string PROPERTIES_PATH = "toolkit.properties";
std::map<std::string, std::string> merchantProperties;

// Function to read properties from a file.
// Must be called before calling other methods in this class.
std::map<std::string, std::string> loadProperties() {
    std::string filePath = PROPERTIES_PATH;
    std::map<std::string, std::string> properties;
    std::ifstream file(filePath);
    std::string line;

    if (file.is_open()) {
        while (std::getline(file, line)) {
            size_t delimiterPos = line.find('=');
            if (delimiterPos != std::string::npos) {
                std::string key = line.substr(0, delimiterPos);
                std::string value = line.substr(delimiterPos + 1);
                merchantProperties[key] = value;
            }
        }
        file.close();
    }
    else {
        std::cerr << "Unable to open file: " << filePath << std::endl;
    }

    return merchantProperties;
}

bool endsWith(const std::string& str, const std::string& suffix) {
    if (suffix.size() > str.size()) return false;
    return std::equal(suffix.rbegin(), suffix.rend(), str.rbegin());
}

char* convertToChar(std::string string)
{
    char* cstr = new char[string.length() + 1]; // allocate memory for the char array
    strcpy(cstr, string.c_str()); // copy the contents of the std::string to the char array
    return cstr;
}

char* getKeyFilePath()
{
    auto keyFileKey = merchantProperties.find("KEY_FILE");

    if (keyFileKey == merchantProperties.end()) {
        std::cerr << "Key File is missing in properties file" << std::endl;
        return (char*)"";
    }

    std::string keyFile = keyFileKey->second;
    std::string keyDirectory = "";

    auto keyDirectoryKey = merchantProperties.find("KEY_DIRECTORY");

    if (keyDirectoryKey != merchantProperties.end()) {
        keyDirectory = keyDirectoryKey -> second;
    }

    if (!endsWith(keyFile, ".p12")) {
        keyFile.append(".p12");
    }

    std::string fullPath = keyDirectory + "/" + keyFile;
    std::ifstream file(fullPath);

    if (!file.is_open()) {
        std::cerr << "The file " << fullPath << " is missing or is not a file." << std::endl;
        return (char*)"";
    }
    else {
        file.close();
    }

    std::cout << "keyFile full path: " << fullPath << std::endl;

    return convertToChar(fullPath);
}

char* getProperty(std::string key)
{
    auto keyFileKey = merchantProperties.find(key);

    if (keyFileKey == merchantProperties.end()) {
        std::cerr << "Property is missing in properties file: " << key << std::endl;
        return (char *) "";
    }

    std::string value = keyFileKey->second;

    return convertToChar(value);
}

