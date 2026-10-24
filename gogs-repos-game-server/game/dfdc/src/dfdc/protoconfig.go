package dfdc

import (
	"DataCfgSlot"
	"fmt"
	"io/ioutil"

	"example.invalid/golang/protobuf/proto"
)

// ProtoConfigs for config data from xls
type ProtoConfigs struct {
	FreeSpinTable          []*DataCfgSlot.FreeSpinTable
	JackpotTable           []*DataCfgSlot.JackpotTable
	MainGameTable          []*DataCfgSlot.MainGameTable
	SlotPattern            []*DataCfgSlot.SlotPattern
	SlotPayLine            []*DataCfgSlot.SlotPayLine
	SlotPayTable           []*DataCfgSlot.SlotPayTable
	SlotSymbolDistribution []*DataCfgSlot.SlotSymbolDistribution
	SymbolAttributes       []*DataCfgSlot.SymbolAttributes
}

// Reload from config file
func (pc *ProtoConfigs) Reload() error {

	if data, err := LoadSlotPattern(); err != nil {
		return err
	} else {
		pc.SlotPattern = data.GetItems()
	}

	if data, err := LoadMainGameTable(); err != nil {
		return err
	} else {
		pc.MainGameTable = data.GetItems()
	}

	if data, err := LoadFreeSpinTable(); err != nil {
		return err
	} else {
		pc.FreeSpinTable = data.GetItems()
	}

	if data, err := LoadSymbolAttributes(); err != nil {
		return err
	} else {
		pc.SymbolAttributes = data.GetItems()
	}

	if data, err := LoadSlotPayLine(); err != nil {
		return err
	} else {
		pc.SlotPayLine = data.GetItems()
	}

	if data, err := LoadSlotPayTable(); err != nil {
		return err
	} else {
		pc.SlotPayTable = data.GetItems()
	}

	if data, err := LoadJackpotTable(); err != nil {
		return err
	} else {
		pc.JackpotTable = data.GetItems()
	}

	if data, err := LoadSlotSymbolDistribution(); err != nil {
		return err
	} else {
		pc.SlotSymbolDistribution = data.GetItems()
	}

	return nil
}

// Print proto config content
func (pc *ProtoConfigs) Print() {

	fmt.Println("=== SlotPattern ===", len(pc.SlotPattern))
	for _, row := range pc.SlotPattern {
		fmt.Println(row)
	}

	fmt.Println("=== MainGameTable ===", len(pc.MainGameTable))
	for _, row := range pc.MainGameTable {
		fmt.Println(row)
	}

	fmt.Println("=== FreeSpinTable ===", len(pc.FreeSpinTable))
	for _, row := range pc.FreeSpinTable {
		fmt.Println(row)
	}

	fmt.Println("=== SymbolAttributes ===", len(pc.SymbolAttributes))
	for _, row := range pc.SymbolAttributes {
		fmt.Println(row)
	}

	fmt.Println("=== SlotPayTable ===", len(pc.SlotPayTable))
	for _, row := range pc.SlotPayTable {
		fmt.Println(row)
	}

	fmt.Println("=== SlotPayLine ===", len(pc.SlotPayLine))
	for _, row := range pc.SlotPayLine {
		fmt.Println(row)
	}

	fmt.Println("=== JackpotTable ===", len(pc.JackpotTable))
	for _, row := range pc.JackpotTable {
		fmt.Println(row)
	}

	fmt.Println("=== SlotSymbolDistribution ===", len(pc.SlotSymbolDistribution))
	for _, row := range pc.SlotSymbolDistribution {
		fmt.Println(row)
	}
}

var protodata *ProtoConfigs

func init() {
	protodata = new(ProtoConfigs)
	protodata.Reload()
}

// LoadSlotPattern load slot pattern
func LoadSlotPattern() (*DataCfgSlot.SlotPattern_ARRAY, error) {

	b, err := ioutil.ReadFile("./protoconfig/SlotPattern.bytes")
	if err != nil {
		return nil, err
	}

	data := new(DataCfgSlot.SlotPattern_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		return nil, err
	}

	return data, nil
}

// LoadMainGameTable load main game table
func LoadMainGameTable() (*DataCfgSlot.MainGameTable_ARRAY, error) {

	b, err := ioutil.ReadFile("./protoconfig/MainGameTable.bytes")
	if err != nil {
		return nil, err
	}

	data := new(DataCfgSlot.MainGameTable_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		return nil, err
	}

	return data, nil
}

// LoadFreeSpinTable load free spin table
func LoadFreeSpinTable() (*DataCfgSlot.FreeSpinTable_ARRAY, error) {

	b, err := ioutil.ReadFile("./protoconfig/FreeSpinTable.bytes")
	if err != nil {
		return nil, err
	}

	data := new(DataCfgSlot.FreeSpinTable_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		return nil, err
	}

	return data, nil
}

// LoadSymbolAttributes load symbol arrtibutes
func LoadSymbolAttributes() (*DataCfgSlot.SymbolAttributes_ARRAY, error) {

	b, err := ioutil.ReadFile("./protoconfig/SymbolAttributes.bytes")
	if err != nil {
		return nil, err
	}

	data := new(DataCfgSlot.SymbolAttributes_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		return nil, err
	}

	return data, nil
}

// LoadSlotPayLine load slot pay line
func LoadSlotPayLine() (*DataCfgSlot.SlotPayLine_ARRAY, error) {

	b, err := ioutil.ReadFile("./protoconfig/SlotPayLine.bytes")
	if err != nil {
		return nil, err
	}

	data := new(DataCfgSlot.SlotPayLine_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		return nil, err
	}

	return data, nil
}

// LoadSlotPayTable load slot pay table
func LoadSlotPayTable() (*DataCfgSlot.SlotPayTable_ARRAY, error) {

	b, err := ioutil.ReadFile("./protoconfig/SlotPayTable.bytes")
	if err != nil {
		return nil, err
	}

	data := new(DataCfgSlot.SlotPayTable_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		return nil, err
	}

	return data, nil
}

// LoadJackpotTable load jackpot table
func LoadJackpotTable() (*DataCfgSlot.JackpotTable_ARRAY, error) {

	b, err := ioutil.ReadFile("./protoconfig/JackpotTable.bytes")
	if err != nil {
		return nil, err
	}

	data := new(DataCfgSlot.JackpotTable_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		return nil, err
	}

	return data, nil
}

// LoadSlotSymbolDistribution load slot symbol distribution
func LoadSlotSymbolDistribution() (*DataCfgSlot.SlotSymbolDistribution_ARRAY, error) {

	b, err := ioutil.ReadFile("./protoconfig/SlotSymbolDistribution.bytes")
	if err != nil {
		return nil, err
	}

	data := new(DataCfgSlot.SlotSymbolDistribution_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		return nil, err
	}

	return data, nil
}
