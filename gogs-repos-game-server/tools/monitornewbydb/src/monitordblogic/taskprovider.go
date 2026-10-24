package monitordblogic

import (
	"monitordblogic/task"
)

type TaskProvider interface {
	GetTask() task.Task
}

//外网工作任务
type ExternalTaskPrvider struct {
}

func (etp *ExternalTaskPrvider) GetTask() task.Task {
	return nil

}

//内网工作任务
type InternalTaskProvider struct {
}

func (itp *InternalTaskProvider) GetTask() task.Task {
	tg := new(task.TaskGroup)
	tg.AddTask(task.NewSelectTask(1, 1))
	tg.AddTask(task.NewUpdateMoneyTask(1,1))
	tg.AddTask(task.NewTelnetTask(120,1))
	return tg
}
