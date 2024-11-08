#! /bin/bash

if [ -z "$ANDROID_KEYSTORE_BASE64" ]; then
    echo "请设置变量"
    exit 1
fi

cd ../src/StarEachOther.Android

dotnet workload restore StarEachOther.Android.csproj

echo $ANDROID_KEYSTORE_BASE64 > keystore-b64.txt

#openssl base64 < origin.keystore | tr -d '\n' | tee keystore.base64.txt
base64 -d keystore-b64.txt > $ANDROID_FILENAME.keystore

dotnet publish -c Release -f net8.0-android -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=$ANDROID_FILENAME.keystore -p:AndroidSigningKeyAlias=$ANDROID_KEYNAME -p:AndroidSigningKeyPass=$ANDROID_KEYPASSWORD -p:AndroidSigningStorePass=$ANDROID_KEYPASSWORD -o bin/publish

mv ./bin/publish/com.yibei333.stareachother-Signed.apk ./bin/GithubStarEachOther_android.apk
