# Firefly��������ܰ�װ��ʹ��

---

## Firefly��CentOS�µİ�װ
> * ��װPython
`����Python-2.7.11`
wget https://www.python.org/ftp/python/2.7.11/Python-2.7.11.tgz
`��ѹ������Ŀ¼`
tar zxvf Python-2.7.11.tgz
cd Python-2.7.11
`���밲װ`
./configure 
make all
make install
make clean
`���������ӣ�ʹϵͳĬ�ϵ�pythonָ��python2.7`
mv /usr/bin/python /usr/bin/python2.6.6
ln -s /usr/bin/python2.7 /usr/bin/python
`���ϵͳ Python ������ָ�� Python2.7 �汾����Ϊyum�ǲ�����Python` 2.7�ģ�����yum��������������������Ҫָ��yum��Python�汾 
\#vi /usr/bin/yum 
���ļ�ͷ���� 
\#!/usr/bin/python 
�ĳ� 
\#!/usr/bin/python2.6.6
> * ��װMySQL
> * ��װmemcached
`��������`
memcached -u root -d
> * ��װpip��python `get-pip.py`
> * ��register.py�ŵ�D��Ȼ��python `register.py`
> * pip install `twisted`
> * pip install `python-memcached`
> * pip install `DBUtils`
> * pip install `affinity`
> * pip install `MySQL-python`
`�����������`
yum install python-devel
yum install mysql-devel
> * ��װFirefly������pip install `firefly`

## Firefly��CentOS�µ�ʹ��
> * firefly-admin.py createproject Test
> * cd Test
> * python startmaster.py
