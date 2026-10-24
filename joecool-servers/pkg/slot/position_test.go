package slot

import (
	"reflect"
	"testing"
)

func TestNewPositionsByPattern(t *testing.T) {
	type args struct {
		pattern string
	}
	tests := []struct {
		name string
		args args
		want []Position
	}{
		{
			name: "Normal",
			args: args{
				pattern: "4_1,3_2,2_1",
			},
			want: []Position{
				Position{Reel: 2, Row: 1},
				Position{Reel: 3, Row: 2},
				Position{Reel: 4, Row: 1},
			},
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if got := NewPositionsByPattern(tt.args.pattern); !reflect.DeepEqual(got, tt.want) {
				t.Errorf("NewPositionsByPattern() = %v, want %v", got, tt.want)
			}
		})
	}
}
