package newpeople

import (
	"fmt"
	"example.invalid/golang/mock/gomock"
	"testing"
)

func TestUse(t *testing.T) {
	mockCtrl := gomock.NewController(t)
	defer mockCtrl.Finish()

	mockDoer := NewMockDoer3(mockCtrl)

	testUser := User{Doer2: mockDoer}

	mockDoer.EXPECT().Show(gomock.Any()).Return("成功... mock msg")
	//mockDoer.EXPECT().Show("mock msg").Return("成功... mock msg")

	msg := testUser.Use()

	fmt.Printf("use() msg : %v", msg)
}

func TestShow(t *testing.T) {
	mockCtrl := gomock.NewController(t)
	//defer mockCtrl.Finish()

	mockObj := NewMockIBaiJialeLogic(mockCtrl)
	mockObj.EXPECT().showIFunc(gomock.Any()).Return("成功... mock msg")

	obj := Baij{b: mockObj}
	msg := obj.show("no mock")

	fmt.Printf("show func : %v", msg)

}
