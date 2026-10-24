package util

import (
	"bytes"
	"fmt"
	"strconv"
)

func Decimal(value float64, n int) float32 {
	var buffer bytes.Buffer
	buffer.WriteString("%.")
	buffer.WriteString(strconv.Itoa(n))
	buffer.WriteString("f")

	value, _ = strconv.ParseFloat(fmt.Sprintf(buffer.String(), value), 64)
	return float32(value)
}
