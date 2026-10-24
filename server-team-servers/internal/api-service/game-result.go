package api

import (
	"context"
	"fmt"

	pb "example.invalid/ServerTeam/Servers/internal/api-service/pb/apisvc"
	"example.invalid/ServerTeam/Servers/internal/apiserver/apilogic/service"
	"example.invalid/grpc"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"
)

// GameResultServiceServer server of game result service
type GameResultServiceServer struct {
	pb.UnimplementedGameResultServiceServer
}

// SettlementV2 下注扣錢
func (s *GameResultServiceServer) SettlementV2(ctx context.Context, in *pb.SettlementReq) (*pb.SettlementRes, error) {

	ok, afterAmount := service.GameResultService.PostSettlementV2(in.AccessToken, in.PlayerID, in.ServerID, in.TradeID, in.UpdateAmount, in.IPAddress, in.TransactionNo, in.BetAmount, in.ValidAmount, in.PayoutAmount, in.TaxAmount, in.SettleTime)

	res := new(pb.SettlementRes)

	if !ok {
		return nil, status.Error(codes.Internal, fmt.Sprintf("SettlementV2 API Error"))
	}

	res.AfterAmount = afterAmount

	return res, nil
}

// MockGameResultServiceClient
type MockGameResultServiceClient struct{}

// SettlementV2 下注扣錢
func (s *MockGameResultServiceClient) SettlementV2(ctx context.Context, in *pb.SettlementReq, opts ...grpc.CallOption) (*pb.SettlementRes, error) {

	res := new(pb.SettlementRes)

	res.AfterAmount = 10000000

	return res, nil
}
