package com.cybersource;

import javax.naming.ConfigurationException;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;

public class PropertiesUtil {
    private static final String propertiesPath = "src/main/resources/toolkit.properties";
    public static Properties merchantProperties;

    public static String getPropertiesPath() {
        return propertiesPath;
    }

    public static Properties loadProperties() throws IOException {
        InputStream merchantPropertiesStream = new FileInputStream(getPropertiesPath());
        merchantProperties = new Properties();
        merchantProperties.load(merchantPropertiesStream);
        return merchantProperties;
    }

    public static String getKeyFilePath() throws ConfigurationException {
        if (merchantProperties.getProperty("KEY_FILE") == null) {
            throw new ConfigurationException("Key File is missing in properties file");
        }

        String keyFile = merchantProperties.getProperty("KEY_FILE");
        String keyDirectory = merchantProperties.getProperty("KEY_DIRECTORY");

        File file;
        if (!keyFile.endsWith(".p12")) {
            file = new File(keyDirectory, keyFile + ".p12");
        } else {
            file = new File(keyDirectory, keyFile);
        }

        String fullPath = file.getAbsolutePath();
        if (!file.isFile()) {
            throw new ConfigurationException(
                    "The file \"" + fullPath + "\" is missing or is not a file.");
        }
        if (!file.canRead()) {
            throw new ConfigurationException(
                    "This application does not have permission to read the file \""
                            + fullPath + "\".");
        }

        return fullPath;
    }
}
