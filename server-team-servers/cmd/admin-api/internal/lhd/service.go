package lhd

import (
	"context"
	"fmt"
	"io"

	"example.invalid/sirupsen/logrus"
	"example.invalid/grpc"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"

	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"

	"example.invalid/ServerTeam/Servers/cmd/admin-api/internal/room"
)

type Service interface {
	Dashboard(int32, int32) (*pb.LHDTableDashboard, error)
	Stream(int32, int32, context.Context, chan<- *pb.LHDTableDashboard) error
	Cheat(int32, int32, int32) error
	SetSpecialRewardCount(int32, int32, int32, int32) error
	IncreaseSysPool(sid int32, tid int32, amount int64) error
	AssignNextAward(sid int32, tid int32, award int32) error
	SetTableDisabled(sid int32, tid int32, disabled bool) error
	SetJackpotEnabled(sid, tid int32, enabled bool) error
	AssignJackpotToRandomPlayer(sid, tid, level int32) error
	AssignJackpotToRecycle(sid, tid, level int32) error
	EditJackpotPool(sid, tid int32, amount int64) error
}

func NewService(
	confM room.ConfMap,
	logger *logrus.Logger,
) Service {
	return &service{
		confM:  confM,
		Logger: logger,
	}
}

type service struct {
	confM  room.ConfMap
	Logger *logrus.Logger
}

func (svc *service) Dashboard(rid, tid int32) (*pb.LHDTableDashboard, error) {

	roomConf, ok := svc.confM[rid]
	if !ok {
		return nil, status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return nil, status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	resp, err := client.TableDashboard(ctx, &pb.LHDQueryTableDashBoard{TableID: tid})
	if err != nil {
		return nil, status.Error(codes.Internal, fmt.Sprintf("gRPC TableDashboard error, %v", err))
	}

	return resp, nil
}

func (svc *service) Stream(rid, tid int32, ctx context.Context, ch chan<- *pb.LHDTableDashboard) error {
	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	stream, err := client.TableDashboardStream(ctx, &pb.LHDQueryTableDashBoard{TableID: tid})
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC TableDashboard error, %v", err))
	}

	for {
		select {
		default:
			m, err := stream.Recv()
			if err != nil {
				if err == io.EOF {
					return nil
				}
			}
			ch <- m
		case <-ctx.Done():
			return nil
		}
	}
}

func (svc *service) Cheat(rid, tid, cnt int32) error {
	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	if _, err := client.Cheat(ctx, &pb.LHDCheatRequest{TableID: tid, KickCnt: cnt}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC Cheat error, %v", err))
	}

	return nil
}

func (svc *service) SetSpecialRewardCount(rid, tid, cnt, rate int32) error {
	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	if _, err := client.SetSpecialRewardCount(ctx, &pb.SetSpecialRewardCountRequest{
		TableID: tid,
		Count:   cnt,
		Rate:    rate,
	}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC SetSpecialRewardCount error, %v", err))
	}

	return nil
}

func (svc *service) IncreaseSysPool(rid, tid int32, amount int64) error {

	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	if _, err := client.IncreaseSysPool(ctx, &pb.IncreaseSysPoolRequest{
		Condition: &pb.QueryCondition{
			Table: tid,
		},
		Amount: amount,
	}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC IncreaseSysPool error, %v", err))
	}

	return nil
}

func (svc *service) AssignNextAward(rid int32, tid int32, award int32) error {
	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	if _, err := client.AssignNextAward(ctx, &pb.LHDAssignNextAwardRequest{
		TableID:   tid,
		NextAward: pb.LHDAward(award),
	}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC AssignNextAward error, %v", err))
	}

	return nil
}

func (svc *service) SetTableDisabled(rid int32, tid int32, disabled bool) error {

	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	if _, err := client.SetTableDisabled(ctx, &pb.LHDSetTableDisabledRequest{
		TableID:  tid,
		Disabled: disabled,
	}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC SetTableDisabled error, %v", err))
	}

	return nil
}

// SetJackpotEnabled
func (svc *service) SetJackpotEnabled(rid int32, tid int32, enabled bool) error {
	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	if _, err := client.SetJackpotEnabled(ctx, &pb.LHDJackpotEnabledRequest{
		TableID: tid,
		Enabled: enabled,
	}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC SetJackpotEnabled error, %v", err))
	}

	return nil
}

// AssignJackpotToRandomPlayer
func (svc *service) AssignJackpotToRandomPlayer(rid, tid, level int32) error {

	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	if _, err := client.AssignJackpotToRandomPlayer(ctx, &pb.LHDAssignJackpotRequest{
		TableID: tid,
		Level:   pb.LHDJackpotLevel(level),
	}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC AssignJackpotToRandomPlayer error, %v", err))
	}

	return nil
}

// AssignJackpotToRecycle
func (svc *service) AssignJackpotToRecycle(rid, tid, level int32) error {

	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	if _, err := client.AssignJackpotToRecycle(ctx, &pb.LHDAssignJackpotRequest{
		TableID: tid,
		Level:   pb.LHDJackpotLevel(level),
	}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC AssignJackpotToRecycle error, %v", err))
	}

	return nil
}

// EditJackpotPool 編輯 JP 獎池
func (svc *service) EditJackpotPool(rid, tid int32, amount int64) error {

	roomConf, ok := svc.confM[rid]
	if !ok {
		return status.Error(codes.NotFound, fmt.Sprintf("room %d not found", rid))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	if _, err := client.EditJackpotPool(ctx, &pb.LHDEditJackpotPoolRequest{
		TableID:   tid,
		FinalPool: amount,
	}); err != nil {
		return status.Error(codes.Internal, fmt.Sprintf("gRPC EditJackpotPool error, %v", err))
	}

	return nil
}
