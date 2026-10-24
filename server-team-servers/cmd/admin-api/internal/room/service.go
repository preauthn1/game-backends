package room

import (
	"context"
	"fmt"

	"example.invalid/sirupsen/logrus"
	"example.invalid/grpc"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"

	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"
)

type Service interface {
	List() ([]RoomInfo, error)
	Info(id int32) (RoomInfo, error)
}

func NewService(
	confL ConfList,
	confM ConfMap,
	logger *logrus.Logger,
) Service {
	return &service{
		confL:  confL,
		confM:  confM,
		Logger: logger,
	}
}

type service struct {
	confL  ConfList
	confM  ConfMap
	Logger *logrus.Logger
}

func (svc *service) List() ([]RoomInfo, error) {
	var rooms []RoomInfo

	for _, roomConf := range svc.confL {

		conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
		if err != nil {
			svc.Logger.Warnf("gRPC connect addr: %s error, %v", roomConf.Addr, err)
			rooms = append(rooms, RoomInfo{
				ID:       roomConf.ServerID,
				Name:     roomConf.Game,
				GameID:   0,
				GameName: roomConf.Game,
				Flag:     0,
				GameCode: roomConf.Game,
				Actived:  false,
			})
			continue
		}
		defer conn.Close()

		client := pb.NewCLIClient(conn)
		r, err := client.RoomInfo(context.TODO(), &pb.Empty{})
		if err != nil {
			svc.Logger.Warnf("gRPC RoomInfo addr: %s error, %v", roomConf.Addr, err)
			rooms = append(rooms, RoomInfo{
				ID:       roomConf.ServerID,
				Name:     roomConf.Game,
				GameID:   0,
				GameName: roomConf.Game,
				Flag:     0,
				GameCode: roomConf.Game,
				Actived:  false,
			})
			continue
		}

		rInfo := RoomInfo{
			ID:       r.ID,
			Name:     r.Name,
			GameID:   r.GameID,
			GameName: r.GameName,
			Flag:     r.Flag,
			GameCode: roomConf.Game,
			Actived:  true,
		}

		for _, t := range r.Tables {
			rInfo.Tables = append(rInfo.Tables, TableInfo{
				ID:      t.ID,
				Gaming:  t.Gaming,
				Summary: t.GameSummary,
				Players: []PlayerInfo{},
				Robots:  []PlayerInfo{},
			})
		}

		rooms = append(rooms, rInfo)
	}

	return rooms, nil
}

func (svc *service) Info(id int32) (RoomInfo, error) {

	roomConf, ok := svc.confM[id]
	if !ok {
		return RoomInfo{}, status.Error(codes.NotFound, fmt.Sprintf("room %d not found", id))
	}

	conn, err := grpc.Dial(roomConf.Addr, grpc.WithInsecure())
	if err != nil {
		return RoomInfo{}, status.Error(codes.Internal, fmt.Sprintf("gRPC connect error, %v", err))
	}
	defer conn.Close()

	client := pb.NewCLIClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	r, err := client.RoomInfo(ctx, &pb.Empty{})
	if err != nil {
		return RoomInfo{}, status.Error(codes.Internal, fmt.Sprintf("gRPC RoomInfo error, %v", err))
	}

	rInfo := RoomInfo{
		ID:       r.ID,
		Name:     r.Name,
		GameID:   r.GameID,
		GameName: r.GameName,
		Flag:     r.Flag,
		GameCode: roomConf.Game,
	}

	for _, t := range r.Tables {
		rInfo.Tables = append(rInfo.Tables, TableInfo{
			ID:      t.ID,
			Gaming:  t.Gaming,
			Summary: t.GameSummary,
			Players: []PlayerInfo{},
			Robots:  []PlayerInfo{},
		})
	}

	{ // Query Player
		resp, err := client.ListPlayer(ctx, &pb.QueryCondition{})
		if err != nil {
			return RoomInfo{}, status.Error(codes.Internal, fmt.Sprintf("gRPC ListPlayer error, %v", err))
		}

		for _, p := range resp.Players {
			if p.Table < 0 {
				continue
			}
			rInfo.PlayerCnt++
			rInfo.Tables[p.Table].Players = append(rInfo.Tables[p.Table].Players, PlayerInfo{
				ID:      p.PlayerID,
				IsRobot: false,
			})
		}
	}

	{ // Query Robot
		resp, err := client.ListRobot(ctx, &pb.QueryCondition{})
		if err != nil {
			return RoomInfo{}, status.Error(codes.Internal, fmt.Sprintf("gRPC ListRobot error, %v", err))
		}
		for _, rbt := range resp.Robots {
			if rbt.Player.Table < 0 {
				continue
			}
			rInfo.RobotCnt++
			rInfo.Tables[rbt.Player.Table].Players = append(rInfo.Tables[rbt.Player.Table].Players, PlayerInfo{
				ID:      rbt.Player.PlayerID,
				IsRobot: true,
			})
		}
	}

	return rInfo, nil
}
