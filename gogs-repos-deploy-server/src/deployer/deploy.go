package deployer

import (
	"io/ioutil"
	"path"
	"log"
	"strings"
	"os/exec"
	"net/http"
	"encoding/json"
	"bytes"
	"fmt"
)

type Deploy struct {
	CONFIG Config
}

func NewDeploy(config Config) *Deploy {

	deploy := new(Deploy)
	deploy.CONFIG = config

	return deploy
}

func(d *Deploy) ActionDeploy(buildName string) map[string]string {
	d.sendNotify("服務器關閉通知 #" + buildName, "即將關閉服務器")
	d.stopServer()
	h := make(map[string]string)

	err := d.gameServerDeploy(buildName)
	if err != nil {
		h["error_message"] = err.Error()
	}
	d.startServer()
	d.sendNotify("服務器啟動通知 #" + buildName, "服務器己重新啟動")
	return h
}

func(d *Deploy) gameServerDeploy(buildName string) (err error){
	CopyDir(d.CONFIG.Release.Root + "\\" + buildName, d.CONFIG.Target.Root)
	gsPath := d.CONFIG.Target.Root + "\\" + d.CONFIG.Target.GSDir
	// list gameserver path
	entries, err := ioutil.ReadDir(gsPath)
	if err != nil {
		return err
	}
	// game deploy, copy 遊戲執行檔
	for _, entry := range entries {
		if entry.IsDir() {
			roomPath := gsPath + "/" + entry.Name()
			d.roomDeploy(roomPath)
		}
	}
	return
}

// copy 遊戲執行檔
func(d *Deploy) roomDeploy(game_dir string) (err error){
	// list room
	entries, err := ioutil.ReadDir(game_dir)
	if err != nil {
		return err
	}
	// room deploy
	for _, entry := range entries {
		gameName:= path.Base(game_dir)
		execFilename := gameName + "." + d.CONFIG.Target.ExtName
		if entry.IsDir() && strings.Contains(entry.Name(), gameName + "_") {
			srcFile := game_dir + "/" + d.CONFIG.Target.SrcDir + "/" + execFilename
			log.Println(srcFile)
			roomDir := game_dir + "/" + entry.Name()
			log.Println(roomDir)
			CopyFile(srcFile, roomDir+ "/" + execFilename)
		}
	}
	return
}

func(d *Deploy) stopServer() {
	for _, command := range d.CONFIG.Stop.Commands {
		cmd := exec.Command(d.CONFIG.Stop.Base + "\\" + command)
		cmd.Dir = d.CONFIG.Stop.Base
		err := cmd.Run()
		if err != nil {
			log.Println(err)
		}
		log.Println(command)
	}
}
func(d *Deploy) startServer() {
	for _, command := range d.CONFIG.Start.Commands {
		cmd := exec.Command(d.CONFIG.Start.Base + "\\" + command)
		cmd.Dir = d.CONFIG.Start.Base
		err := cmd.Run()
		if err != nil {
			log.Println(err)
		}
		log.Println(command)
	}
}
func(d *Deploy) restartServer(){
	d.stopServer()
	d.startServer()
}

type response1 struct {
	Page   int
	Fruits []string
}
type NotifyData struct {
	Title string `json:"title"`
	Text string `json:"text"`
	ThemeColor string `json:"themeColor"`
}
func(d *Deploy) sendNotify(title string, text string) {

	res1D := &response1{
		Page:   1,
		Fruits: []string{"apple", "peach", "pear"}}
	res1B, _ := json.Marshal(res1D)
	fmt.Println(string(res1B))

	data := &NotifyData{
		Title:title,
		Text:text,
		ThemeColor:"EA4300"}
	post, _ := json.Marshal(data)
	fmt.Println(string(post))
	resp, err := http.Post(d.CONFIG.Notify.WebHook, "application/json", bytes.NewBuffer(post))
	log.Println("http url : " + d.CONFIG.Notify.WebHook + ", PostData : " + string(post) + ", resp : " + resp.Status)
	if err != nil {
		log.Println("exection : " + err.Error())
	}


	/*
	Launch curl request
-H "Content-Type: application/json"
-X POST
-d {"text":"A new version of Sample project  was published","themeColor":"EA4300","title":"New Version published"}
to  https://example.invalid/webhook/1ef22373-712e-43fb-ab7e-f7e1bcd2d4d6@e16cc667-9473-4fc9-9b30-211cdec6647e/JenkinsCI/fec81138d6e34c658b767e4b7d92285d/4a62c4d6-f68a-464f-bf6e-eed5003fccff
	 */


}