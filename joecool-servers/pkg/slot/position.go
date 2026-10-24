package slot

import (
	"fmt"
	"sort"
	"strconv"
	"strings"
)

// Position GameMap position
type Position struct {
	Reel int32
	Row  int32
}

func (p Position) String() string {
	return fmt.Sprintf("Reel: %d, Row: %d", p.Reel, p.Row)
}

// NewPositionsByPattern parse string "{ReelNum}:{RowNum},..." to Positions
func NewPositionsByPattern(pattern string) []Position {
	strs := strings.Split(pattern, ",")

	positions := make([]Position, 0, len(strs))

	for _, str := range strs {
		data := strings.Split(str, "_")

		if len(data) != 2 {
			continue
		}

		pos := Position{}

		if val, err := strconv.ParseFloat(data[0], 8); err != nil {
			continue
		} else {
			pos.Reel = int32(val)
		}

		if val, err := strconv.ParseFloat(data[1], 8); err != nil {
			continue
		} else {
			pos.Row = int32(val)
		}

		positions = append(positions, pos)
	}

	sort.Slice(positions, func(i, j int) bool {
		return positions[i].Reel < positions[j].Reel
	})

	return positions
}
