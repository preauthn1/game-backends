package dfdc

import (
	"DataCfgSlot"
	"fmt"
	"io/ioutil"
	"testing"

	"example.invalid/golang/protobuf/proto"
)

func TestDFDCLogic_GetBonusTable(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/BonusTable.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.BonusTable_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetFreeSpinTable(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/FreeSpinTable.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.FreeSpinTable_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetJackPotTable(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/JackPotTable.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.JackpotTable_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetMainGameTable(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/MainGameTable.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.MainGameTable_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetPickGameTable(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/PickGameTable.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.PickGameTable_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetSlotPattern(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/SlotPattern.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.SlotPattern_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetSlotPayLine(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/SlotPayLine.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.SlotPayLine_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetSlotPayTable(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/SlotPayTable.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.SlotPayTable_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetSlotSymbolDistribution(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/SlotSymbolDistribution.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.SlotSymbolDistribution_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}

func TestDFDCLogic_GetSymbolAttributes(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/SymbolAttributes.bytes")
	if err != nil {
		t.Error(err)
	}

	msg := new(DataCfgSlot.SymbolAttributes_ARRAY)
	if err := proto.Unmarshal(b, msg); err != nil {
		t.Error(err)
	}

	fmt.Println(len(msg.Items))

	for _, item := range msg.Items {
		fmt.Println(item)
	}
}
