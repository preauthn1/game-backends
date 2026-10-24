package di

import (
	"fmt"
	"io"
	"os"
	"runtime"
	"time"

	"example.invalid/sirupsen/logrus"
	"example.invalid/spf13/viper"
)

func NewLoguraseLogger() (*logrus.Logger, error) {

	type output struct {
		Type string `yaml:"type"`
	}

	type conf struct {
		Level  string   `yaml:"level"`
		Output []output `yaml:"output"`
	}

	c := new(conf)

	if err := viper.UnmarshalKey("logger", c); err != nil {
		return nil, err
	}

	logger := logrus.New()

	{ // Level
		lv, err := logrus.ParseLevel(c.Level)
		if err != nil {
			return nil, err
		}
		logger.SetLevel(lv)
	}

	{ // Output
		var outputs []io.Writer

		for _, o := range c.Output {
			switch o.Type {
			case "stdout":
				outputs = append(outputs, os.Stdout)
			}
		}

		logger.SetOutput(io.MultiWriter(outputs...))
	}

	{ // Formatter
		switch viper.GetString("logger.formatter") {
		case "text":
			logger.SetFormatter(&logrus.TextFormatter{
				TimestampFormat: time.RFC3339,
				FullTimestamp:   true,
				ForceColors:     true,
				CallerPrettyfier: func(f *runtime.Frame) (function string, file string) {
					return "", fmt.Sprintf("%s:%d", f.File, f.Line)
				},
			})
		case "json":
			logger.SetFormatter(&logrus.JSONFormatter{})
		}
	}

	logger.SetReportCaller(viper.GetBool("logger.reportCaller"))

	return logger, nil
}
