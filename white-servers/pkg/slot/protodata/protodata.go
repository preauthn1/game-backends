package protodata

import (
	"fmt"
	"io/ioutil"
	"os"

	"example.invalid/golang/protobuf/proto"
)

// DefaultPaths 預設讀取路徑
var DefaultPaths = []string{"./protodata", "../../protodata", "./protoconfig", "../../protoconfig"}

// ProtoOption 各個表的選項
type ProtoOption int

// ProtoOptions
const (
	OptSlotPattern ProtoOption = iota
	OptMainGameTable
	OptSlotPayLine
	OptSlotPayTable
	OptSlotSymbolDistribution
	OptSymbolAttributes
	OptJackpotTable
	OptBonusTable
	OptPickGameTable
	OptRespinTable
	OptFreeSpinTable
)

// ProtoData for config data from xls
type ProtoData struct {
	basicPath              string
	opts                   map[ProtoOption]bool
	SlotPattern            []*SlotPattern
	SymbolAttributes       []*SymbolAttributes
	MainGameTable          []*MainGameTable
	SlotSymbolDistribution []*SlotSymbolDistribution
	JackpotTable           []*JackpotTable
	SlotPayLine            []*SlotPayLine
	SlotPayTable           []*SlotPayTable
	BonusTable             []*BonusTable
	PickGameTable          []*PickGameTable
	RespinTable            []*RespinTable
	FreeSpinTable          []*FreeSpinTable
}

// NewProtoData create ProtoData by given path and the proto option what you want to load
func NewProtoData(path string, opts ...ProtoOption) (*ProtoData, error) {
	// Check path folder exist
	fi, err := os.Stat(path)
	if err != nil {
		return nil, err
	}

	if !fi.IsDir() {
		return nil, fmt.Errorf("%s is not a folder", path)
	}

	data := new(ProtoData)
	data.basicPath = path
	data.opts = make(map[ProtoOption]bool)
	for _, opt := range opts {
		data.opts[opt] = true
	}

	return data, nil
}

// Reload from config file
func (pd *ProtoData) Reload() error {

	// SlotPattern
	if val, ok := pd.opts[OptSlotPattern]; val && ok {
		i := new(SlotPattern_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/SlotPattern.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.SlotPattern = i.GetItems()
	}

	// SymbolAttributes
	if val, ok := pd.opts[OptSymbolAttributes]; val && ok {
		i := new(SymbolAttributes_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/SymbolAttributes.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.SymbolAttributes = i.GetItems()
	}

	// MainGameTable
	if val, ok := pd.opts[OptMainGameTable]; val && ok {
		i := new(MainGameTable_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/MainGameTable.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.MainGameTable = i.GetItems()
	}

	// SlotSymbolDistribution
	if val, ok := pd.opts[OptSlotSymbolDistribution]; val && ok {
		i := new(SlotSymbolDistribution_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/SlotSymbolDistribution.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.SlotSymbolDistribution = i.GetItems()
	}

	// JackpotTable
	if val, ok := pd.opts[OptJackpotTable]; val && ok {
		i := new(JackpotTable_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/JackpotTable.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.JackpotTable = i.GetItems()
	}

	// SlotPayLine
	if val, ok := pd.opts[OptSlotPayLine]; val && ok {
		i := new(SlotPayLine_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/SlotPayLine.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.SlotPayLine = i.GetItems()
	}

	// SlotPayTable
	if val, ok := pd.opts[OptSlotPayTable]; val && ok {
		i := new(SlotPayTable_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/SlotPayTable.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.SlotPayTable = i.GetItems()
	}

	// BonusTable
	if val, ok := pd.opts[OptBonusTable]; val && ok {
		i := new(BonusTable_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/BonusTable.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.BonusTable = i.GetItems()
	}

	// PickGameTable
	if val, ok := pd.opts[OptPickGameTable]; val && ok {
		i := new(PickGameTable_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/PickGameTable.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.PickGameTable = i.GetItems()
	}

	// RespinTable
	if val, ok := pd.opts[OptRespinTable]; val && ok {
		i := new(RespinTable_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/RespinTable.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.RespinTable = i.GetItems()
	}

	// FreeSpinTable
	if val, ok := pd.opts[OptFreeSpinTable]; val && ok {
		i := new(FreeSpinTable_ARRAY)
		if err := pd.Load(fmt.Sprintf("%s/FreeSpinTable.bytes", pd.basicPath), i); err != nil {
			return err
		}
		pd.FreeSpinTable = i.GetItems()
	}

	return nil
}

// Print proto config content
func (pd *ProtoData) Print() {

	// SlotPattern
	{
		fmt.Println("=== SlotPattern ===", len(pd.SlotPattern))
		for _, row := range pd.SlotPattern {
			fmt.Println(row)
		}
	}

	// SymbolAttributes
	{
		fmt.Println("=== SymbolAttributes ===", len(pd.SymbolAttributes))
		for _, row := range pd.SymbolAttributes {
			fmt.Println(row)
		}
	}

	// MainGameTable
	{
		fmt.Println("=== MainGameTable ===", len(pd.MainGameTable))
		for _, row := range pd.MainGameTable {
			fmt.Println(row)
		}
	}

	// SlotSymbolDistribution
	{
		fmt.Println("=== SlotSymbolDistribution ===", len(pd.SlotSymbolDistribution))
		for _, row := range pd.SlotSymbolDistribution {
			fmt.Println(row)
		}
	}

	// JackpotTable
	{
		fmt.Println("=== JackpotTable ===", len(pd.JackpotTable))
		for _, row := range pd.JackpotTable {
			fmt.Println(row)
		}
	}

	// SlotPayLine
	{
		fmt.Println("=== SlotPayLine ===", len(pd.SlotPayLine))
		for _, row := range pd.SlotPayLine {
			fmt.Println(row)
		}
	}

	// SlotPayTable
	{
		fmt.Println("=== SlotPayTable ===", len(pd.SlotPayTable))
		for _, row := range pd.SlotPayTable {
			fmt.Println(row)
		}
	}

	// BonusTable
	{
		fmt.Println("=== BonusTable ===", len(pd.BonusTable))
		for _, row := range pd.BonusTable {
			fmt.Println(row)
		}
	}

	// PickGameTable
	{
		fmt.Println("=== PickGameTable ===", len(pd.PickGameTable))
		for _, row := range pd.PickGameTable {
			fmt.Println(row)
		}
	}

	// RespinTable
	{
		fmt.Println("=== RespinTable ===", len(pd.RespinTable))
		for _, row := range pd.RespinTable {
			fmt.Println(row)
		}
	}

	// FreeSpinTable
	{
		fmt.Println("=== FreeSpinTable ===", len(pd.FreeSpinTable))
		for _, row := range pd.FreeSpinTable {
			fmt.Println(row)
		}
	}

}

// Load proto data by path & type
func (pd *ProtoData) Load(path string, i interface{}) error {
	b, err := ioutil.ReadFile(path)
	if err != nil {
		return err
	}

	if err := proto.Unmarshal(b, i.(proto.Message)); err != nil {
		return err
	}

	return nil
}
