package deploy

import (
	"reflect"
	"testing"
)

var cfg = Config{
	Server: struct {
		Port string
	}{
		Port: "888",
	},
	Release: struct {
		Root string
	}{
		Root: "",
	},
	Target: struct {
		Root    string
		GSDir   string
		SrcDir  string
		ExtName string
	}{
		Root:    "",
		GSDir:   "",
		SrcDir:  "",
		ExtName: "",
	},
	Stop: struct {
		Base     string
		Commands []string
	}{
		Base: "",
		Commands: []string{
			"",
		},
	},
	Start: struct {
		Base     string
		Commands []string
	}{
		Base: "",
		Commands: []string{
			"",
		},
	},
	Notify: struct {
		WebHook string
	}{
		WebHook: "",
	},
}

func TestNew(t *testing.T) {

	type args struct {
		config Config
	}
	tests := []struct {
		name string
		args args
		want *Deploy
	}{
		{
			name: "New",
			args: args{
				config: cfg,
			},
			want: &Deploy{
				CONFIG: cfg,
			},
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := New(tt.args.config); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("NewDeploy() = %v, want %v", got, tt.want)
			}
		})
	}
}
