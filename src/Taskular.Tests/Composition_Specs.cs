// Copyright 2007-2014 Chris Patterson
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.
namespace Taskular.Tests
{
    namespace Composition_Specs
    {
        using System;
        using System.Threading.Tasks;
        using NUnit.Framework;


        [TestFixture]
        public class Composing_on_top_of_composers
        {
            [Test]
            public async void Should_support_cool_stuff()
            {
                SideCarrier<B> factorySideCarrier = new FactorySideCarrier<B>();
                var carrier = new AircraftCarrier();

                Payload<A> payload = new PayloadImpl<A>(new A());

                Task<Payload<A>> task = ComposerFactory.Compose(payload, composer => { factorySideCarrier.Compose(composer, carrier); });

                await task;
            }


            class FactorySideCarrier<T> :
                SideCarrier<T>
                where T : new()
            {
                public void Compose<TPayload>(Composer<Payload<TPayload>> composer, Carrier<Tuple<TPayload, T>> next)
                {
                    composer.ComposeTask(p => composer.Payload.MergeRight(new T()), next.Compose);
                }
            }


            class AircraftCarrier :
                Carrier<Tuple<A, B>>

            {
                public void Compose(Composer<Payload<Tuple<A, B>>> composer)
                {
                    composer.Execute(() => Console.WriteLine(composer.Payload.Data.Item2.Value));
                }
            }


            class A
            {
            }


            class B
            {
                public B()
                {
                    Value = "Hello";
                }

                public string Value { get; private set; }
            }
        }


        interface Carrier<T>
        {
            void Compose(Composer<Payload<T>> composer);
        }


        interface SideCarrier<T>
        {
            void Compose<TPayload>(Composer<Payload<TPayload>> composer, Carrier<Tuple<TPayload, T>> next);
        }


        interface Payload
        {
        }


        interface Payload<out T> :
            Payload
        {
            T Data { get; }
        }


        class PayloadImpl<T> :
            Payload<T>
        {
            readonly T _data;

            public PayloadImpl(T data)
            {
                _data = data;
            }

            public T Data
            {
                get { return _data; }
            }
        }


        static class Extensions
        {
            public static Payload<Tuple<T, TRight>> MergeRight<T, TRight>(this Payload<T> payload, TRight right)
            {
                return PayloadProxy.Create(payload, payload.Data, right);
            }

            public static Composer<Payload<T>> ComposeCarrier<T, TPayload>(this Composer<Payload<T>> composer,
                Carrier<Tuple<T, TPayload>> next,
                Func<Payload<T>, Payload<Tuple<T, TPayload>>> payload)
            {
                return composer.ComposeTask(payload, next.Compose);
            }


            static class PayloadProxy
            {
                public static Payload<Tuple<TLeft, TRight>> Create<TLeft, TRight>(Payload payload, TLeft left,
                    TRight right)
                {
                    return new PayloadProxy<Tuple<TLeft, TRight>>(payload, Tuple.Create(left, right));
                }
            }


            class PayloadProxy<T> :
                Payload<T>
            {
                T _data;

                public PayloadProxy(Payload payload, T data)
                {
                    _data = data;
                }

                public T Data
                {
                    get { return _data; }
                }
            }
        }
    }
}