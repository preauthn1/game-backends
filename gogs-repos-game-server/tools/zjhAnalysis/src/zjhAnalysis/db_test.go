package zjhAnalysis

import (
	"encoding/csv"
	"log"
	"os"
	"testing"
)

var data = [][]string{{"Line1", "Hello Readers of"}, {"Line2", "example.invalid"}}

func TestData(t *testing.T) {
	rows := [][]string{
		{"Name", "City", "Language"},
		{"Pinky", "London", "Python"},
		{"Nicky", "Paris", "Golang"},
		{"Micky", "Tokyo", "Php"},
	}

	csvfile, err := os.Create("test.csv")

	if err != nil {
		log.Fatalf("failed creating file: %s", err)
	}

	csvwriter := csv.NewWriter(csvfile)

	for _, row := range rows {
		_ = csvwriter.Write(row)
	}

	csvwriter.Flush()

	csvfile.Close()
}
