k8s setup

* reset k8s cluster
```
kubeadm reset
systemctl stop kubelet
systemctl stop docker
rm -rf /var/lib/cni/
rm -rf /var/lib/kubelet/*
rm -rf /etc/cni/
ifconfig cni0 down
ifconfig flannel.1 down
ifconfig docker0 down
ip link delete cni0
ip link delete flannel.1
##重启kubelet
systemctl restart kubelet
##重启docker
systemctl restart docker
```

* initial k8s cluster
> kubeadm init --pod-network-cidr=0.0.0.0/16
> kubeadm join every nodes

* install flannel
> kubectl apply -f https://example.invalid/coreos/flannel/master/Documentation/kube-flannel.yml
可以透過檢查 coredns pod 的運行狀態來判斷網路插件是否正常運行

* install MetalLB(Helm)
```
helm install metallb stable/metallb -f metallb/value.yaml
or
helm upgrade metallb stable/metallb -f metallb/value.yaml
```

* install MetalLB(old)
> kubectl apply -f https://example.invalid/google/metallb/v0.8.3/manifests/metallb.yaml
> kubectl apply -f metallb/config.yaml
```
apiVersion: v1
kind: ConfigMap
metadata:
  namespace: metallb-system
  name: config
data:
  config: |
    address-pools:
    - name: default
      protocol: layer2
      auto-assign: true
      addresses:
      - 0.0.0.0-0.0.0.0
    # - name: production
    #   auto-assign: false
    #   avoid-buggy-ips: true
    #   addresses:
    #   - 0.0.0.0/24
```

* install helm
```
wget https://get.helm.sh/helm-v2.14.1-linux-amd64.tar.gz
tar -zxvf helm-v2.14.1-linux-amd64.tar.gz
sudo mv linux-amd64/helm /usr/local/bin/helm
```
> kubectl -n kube-system create serviceaccount tiller
> kubectl create clusterrolebinding tiller --clusterrole cluster-admin --serviceaccount=kube-system:tiller
> helm init --service-account tiller

* install nginx-ingress
> helm install nginx-ingress stable/nginx-ingress -f nginx-ingress\values.yaml
> kubectl edit cm nginx-ingress-controller
```
# allow header underscores
enable-underscores-in-headers: "true"
ignore-invalid-headers: "false"
```
* install nfs-client
> helm install nfs-client stable/nfs-client-provisioner -f storage\fast.yaml 
```
storageClass:
  name: fast
  defaultClass: true
nfs:
  server: 0.0.0.0
  path: /hd2/nfsshare
```

* install minio
> helm install --name minio -f minio-distributed\values.yaml stable/minio
```
mode: distributed
replicas: 4
accessKey: minio
secretKey: minio123
service: 
  type: LoadBalancer
persistence:
  storageClass: fast
  size: 10Gi
```

* install cert-manager
> kubectl apply --validate=false -f https://example.invalid/jetstack/cert-manager/release-0.12/deploy/manifests/00-crds.yaml
> kubectl create namespace cert-manager
> helm repo add jetstack https://example.invalid
> helm repo update
> helm install --name cert-manager --namespace cert-manager --version v0.12.0 jetstack/cert-manager
> kubectl get pods --namespace cert-manager

* create issuer
> kubectl apply -f acme-issuer\acme-issuer.yaml
```
apiVersion: example.invalid/v1alpha2
kind: Issuer
metadata:
  name: acme-issuer
spec:
  acme:
    email: user@example.invalid
    server: https://example.invalid/directory
    privateKeySecretRef:
      name: acme-issuer-account-key
    solvers:
    - http01:
        ingress:
          class: nginx
```
> kubectl describe issuer acme-issuer
* create certificate
> kubectl describe issuer acme-issuer
```
apiVersion: example.invalid/v1alpha2
kind: Certificate
metadata:
  name: docker-registry
spec:
  secretName: docker-registry-tls-certificate
  duration: 2m
  renewBefore: 1m
  issuerRef:
    name: acme-issuer
    kind: Issuer
    group: example.invalid
  dnsNames:
  - example.invalid
```
> kubectl describe certificate docker-registry
> kubectl describe secret docker-registry-tls-certificate

* install docker-registry
> kubectl apply -f docker-registry\secret.yaml
```
apiVersion: v1
kind: Secret
metadata:
  name: docker-registry
type: Opaque
data:
  HTPASSWD: [REDACTED]
```
> kubectl apply -f docker-registry\config.yaml
```
apiVersion: v1
kind: ConfigMap
metadata:
  name: docker-registry
data:
  registry-config.yml: |
    version: 0.1
    log:
      fields:
        service: registry
    storage:
      cache:
        blobdescriptor: inmemory
      filesystem:
        rootdirectory: /var/lib/registry
    http:
      addr: :5000
      headers:
        X-Content-Type-Options: [nosniff]
    auth:
      htpasswd:
        [REDACTED] basic-realm
        path: /auth/htpasswd
    health:
      storagedriver:
        enabled: true
        interval: 10s
        threshold: 3
```
> kubectl apply -f docker-registry\deployment.yaml
```
apiVersion: v1
kind: Pod
metadata:
  name: docker-registry
  labels:
    name: docker-registry
spec:
  volumes:
    - name: config
      configMap:
        name: docker-registry
        items:
          - key: registry-config.yml
            path: config.yml
    - name: htpasswd
      secret:
        [REDACTED] docker-registry
        items:
        - key: HTPASSWD
          path: htpasswd
    - name: storage
      emptyDir: {}
  containers:
    - name: docker-registry
      image: registry
      imagePullPolicy: IfNotPresent
      ports:
        - name: http
          containerPort: 5000
          protocol: TCP
      volumeMounts:
        - name: config
          mountPath: /etc/docker/registry
          readOnly: true
        - name: htpasswd
          mountPath: /auth
          readOnly: true
        - name: storage
          mountPath: /var/lib/registry
```
> kubectl apply -f docker-registry\service.yaml
```
apiVersion: v1
kind: Service
metadata:
  name: docker-registry
spec:
  type: ClusterIP
  ports:
    - name: http
      protocol: TCP
      port: 5000
      targetPort: 5000

  selector:
    name: docker-registry
```
> kubectl apply -f docker-registry\ingress.yaml
```
apiVersion: extensions/v1beta1
kind: Ingress
metadata:
  name: docker-registry
  annotations:
    example.invalid/ingress.class: nginx
    example.invalid/proxy-body-size: "0"
    example.invalid/issuer: acme-issuer
spec:
  tls:
  - hosts:
    - example.invalid
    secretName: docker-registry-tls-certificate
  rules:
  - host: example.invalid
    http:
      paths:
      - backend:
          serviceName: docker-registry
          servicePort: 5000
```

* create busybox utility
> kubectl apply -f busybox.yaml
```
apiVersion: v1
kind: Pod
metadata:
  name: busybox
  namespace: default
spec:
  containers:
  - name: busybox
    image: busybox
    command:
      - sleep
      - "3600"
    imagePullPolicy: IfNotPresent
  restartPolicy: Always
```

> # 測試內部域名是否能夠解析
> kubectl exec -ti busybox -- ping minio.default.svc.cluster.local
> # 基本狀態瀏覽
> kubectl get pod,svc --all-namespaces

* gitlab runner
> # docker registry secret
> kubectl create secret docker-registry regcred --docker-server=https://example.invalid/ --docker-username=admin --docker-password=[REDACTED] --docker-email=user@example.invalid -n gitlab-ci-runner
> # minio access secret
> kubectl create secret generic s3access --from-literal=accesskey="minio" --from-literal=secretkey="minio123" -n gitlab-ci-runner
> # create RBAC ServiceAccount
> kubectl apply -f gitlab-ci-runner/rbac.yaml
```
apiVersion: v1
kind: ServiceAccount
metadata:
  name: gitlab-ci
  namespace: gitlab-ci-runner
---
kind: Role
apiVersion: example.invalid/v1
metadata:
  name: gitlab-ci
  namespace: gitlab-ci-runner
rules:
  - apiGroups: [""]
    resources: ["*"]
    verbs: ["*"]
---
kind: RoleBinding
apiVersion: example.invalid/v1
metadata:
  name: gitlab-ci
  namespace: gitlab-ci-runner
subjects:
  - kind: ServiceAccount
    name: gitlab-ci
    namespace: gitlab-ci-runner
roleRef:
  kind: Role
  name: gitlab-ci
  apiGroup: example.invalid
```
> helm install shared-1 gitlab/gitlab-runner --namespace gitlab-ci-runner -f gitlab-ci-runner\values.yaml
> helm install shared-2 gitlab/gitlab-runner --namespace gitlab-ci-runner -f gitlab-ci-runner\values.yaml
```
image: gitlab/gitlab-runner:alpine-v12.1.0
imagePullPolicy: IfNotPresent
gitlabUrl: http://example.invalid:1688/
runnerRegistrationToken: [REDACTED]"
unregisterRunners: true
terminationGracePeriodSeconds: 3600
concurrent: 10
checkInterval: 5
rbac:
  create: false
  clusterWideAccess: false
  serviceAccountName: gitlab-ci
metrics:
  enabled: true
runners:
  image: example.invalid/go-tools:1.12.13
  imagePullSecrets: [regcred]
  imagePullPolicy: "if-not-present"
  tags: "golang"
  privileged: false
  namespace: gitlab-ci-runner
  pollTimeout: 180
  outputLimit: 4096
  cache: 
    cacheType: s3
    cachePath: "gitlab_runner"
    cacheShared: true
    s3ServerAddress: example.invalid:9000
    s3BucketName: gitlab-runner
    s3BucketLocation: us-east-1
    s3CacheInsecure: true
    secretName: s3access
  serviceAccountName: gitlab-ci
securityContext:
  fsGroup: 65533
  runAsUser: 100
```
# 安裝 reloader
> kubectl apply -f https://example.invalid/stakater/Reloader/master/deployments/kubernetes/reloader.yaml

# 安裝 dashboard https://example.invalid/kubernetes/dashboard
> kubectl apply -f https://example.invalid/kubernetes/dashboard/v2.0.0-rc6/aio/deploy/recommended.yaml

# 創建 最高權限的 ServiceAccount
> kubectl create -f role/admin-role.yaml

# 獲取 admin-token 的secret名字
$ kubectl -n kube-system get secret|grep admin-token
# 獲取 token 的值
$ kubectl -n kube-system describe secret admin-token-nwphb
# 訪問 Dashboard
> http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/