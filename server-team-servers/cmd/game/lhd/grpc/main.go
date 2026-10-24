package main

import (
	"context"
	"encoding/json"
	"io"
	"log"

	"example.invalid/grpc"

	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"
)

func main() {

	conn, err := grpc.Dial(":3100", grpc.WithInsecure())
	if err != nil {
		panic(err)
	}

	c := pb.NewGameAdminLHDClient(conn)

	ctx, cancel := context.WithCancel(context.TODO())
	defer cancel()

	stream, err := c.TableDashboardStream(ctx, &pb.LHDQueryTableDashBoard{TableID: 0})
	if err != nil {
		panic(err)
	}

	for {
		select {
		case <-ctx.Done():
			return
		default:
			m, err := stream.Recv()
			if err != nil {
				if err == io.EOF {
					return
				}
				log.Println(err)
				return
			}

			b, err := json.MarshalIndent(m, "", "  ")
			if err != nil {
				log.Println(err)
			}

			log.Println(string(b))
		}
	}
}
