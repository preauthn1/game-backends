package slot

import (
	"fmt"
	"sort"
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// Position GameMap position
type Position struct {
	Reel int32
	Row  int32
}

func (p Position) String() string {
	return fmt.Sprintf("Reel: %d, Row: %d", p.Reel, p.Row)
}

// PayLine for parsed Proto data
type PayLine struct {
	Origin    *protodata.SlotPayLine
	Positions []Position
}

func (line *PayLine) String() string {
	return fmt.Sprintf("%d:(%s)", line.Origin.GetLineId(), line.Origin.GetContent())
}

// NewPayLine create PayLine
func NewPayLine(origin *protodata.SlotPayLine) *PayLine {
	if origin == nil {
		return nil
	}

	line := new(PayLine)
	line.Origin = origin

	line.GeneratePosition()

	return line
}

// GeneratePosition parsed proto data content to Position array
func (line *PayLine) GeneratePosition() {
	if line.Origin == nil {
		return
	}

	positions := strings.Split(line.Origin.GetContent(), ",")

	line.Positions = make([]Position, 0, len(positions))

	for _, point := range positions {
		data := strings.Split(point, "_")
		if len(data) != 2 {
			continue
		}

		position := Position{}

		reel, err := strconv.ParseInt(data[0], 10, 32)
		if err != nil {
			continue
		}

		row, err := strconv.ParseInt(data[1], 10, 32)
		if err != nil {
			continue
		}

		position.Reel = int32(reel)
		position.Row = int32(row)

		line.Positions = append(line.Positions, position)
	}

	sort.Slice(line.Positions, func(i, j int) bool {
		return line.Positions[i].Reel < line.Positions[j].Reel
	})
}

// Reverse Payline reverse left to right
func (line *PayLine) Reverse() *PayLine {

	// sort desc
	sort.Slice(line.Positions, func(i, j int) bool {
		return line.Positions[i].Reel > line.Positions[j].Reel
	})

	// reset reel
	for i := 0; i < len(line.Positions); i++ {
		line.Positions[i].Reel = int32(i + 1)
	}

	return line
}
