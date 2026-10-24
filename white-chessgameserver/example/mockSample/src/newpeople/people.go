package newpeople

type User struct {
	Doer2 Doer3
}

type Doer3 interface {
	DoSomething(int, string) string
	Show(string) string
	Display(string) string
}

type Baij struct {
	a string
	b IBaiJialeLogic
}

type IBaiJialeLogic interface {
	showIFunc(string) string
}

func (bjl *Baij) show(str string) string {
	//fmt.Printf("測試interface func : %v", str)
	return bjl.b.showIFunc(str)
}

func Display(str string) string {
	return str
}

func (u *User) Use() string {
	//return u.Doer2.DoSomething(123, "Hello GoMock")
	return u.Doer2.Show("mock msg")
}
