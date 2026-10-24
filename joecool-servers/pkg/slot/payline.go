package slot

import (
	"fmt"
	"sort"

	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

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

	line.Positions = NewPositionsByPattern(line.Origin.GetContent())
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
